using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using haru.market.Models;
using haru.market.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace haru.market.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public CheckoutController(OrderService orderService, ProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string userUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "";
            if (string.IsNullOrEmpty(userUid)) return RedirectToAction("Login", "Account");

            var activeCartItems = await _productService.GetCartItemsAsync(userUid);
            
            if (!activeCartItems.Any()) return RedirectToAction("Shop", "Home");

            ViewBag.CartItems = activeCartItems;

            var checkoutSetup = new OrderPlacementViewModel
            {
                CustomerName = string.Empty,
                CustomerEmail = User.Identity?.Name ?? string.Empty,
                ShippingAddress = string.Empty,
                Items = new List<OrderItemModel>()
            };

            foreach (var item in activeCartItems)
            {
                checkoutSetup.Items.Add(new OrderItemModel
                {
                    ProductId = string.IsNullOrEmpty(item.Size) ? item.ProductId : $"{item.ProductId}_{item.Size}",
                    
                    ProductName = $"{item.ProductName} (Size: {item.Size})",
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }

            return View(checkoutSetup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(OrderPlacementViewModel model, string PaymentMethod, string City, string PostalCode, string PhoneNumber)
        {
            string userUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            string fullAddress = model.ShippingAddress ?? "";
            if (!string.IsNullOrEmpty(City)) fullAddress += $", {City}";
            if (!string.IsNullOrEmpty(PostalCode)) fullAddress += $" {PostalCode}";
            if (!string.IsNullOrEmpty(PhoneNumber)) fullAddress += $" | Phone: {PhoneNumber}";
            
            model.ShippingAddress = fullAddress;

            if (model.Items == null || !model.Items.Any())
            {
                var activeCartItems = await _productService.GetCartItemsAsync(userUid);
                model.Items = activeCartItems.Select(i => new OrderItemModel {

                    ProductId = string.IsNullOrEmpty(i.Size) ? i.ProductId : $"{i.ProductId}_{i.Size}", 
                    
                    ProductName = $"{i.ProductName} (Size: {i.Size})", 
                    Price = i.Price, 
                    Quantity = i.Quantity
                }).ToList();
            }
            try
            {
                string orderToken = await _orderService.CreateOrderAsync(model);

                await ClearUserCartAsync(userUid);

                if (PaymentMethod == "Online")
                {
                    string xenditCheckoutUrl = await _orderService.CreateXenditInvoiceAsync(orderToken, model);
                    _orderService.DispatchInvoiceBackground(orderToken, model);
                    return Redirect(xenditCheckoutUrl);
                }
                else 
                {
                    return RedirectToAction("Success");
                }
            }
            catch (System.Exception ex)
            {
                return Content($"Checkout Aborted by Transaction Protection: {ex.Message}");
            }
        }

        private async Task ClearUserCartAsync(string userUid)
        {
            var db = _productService.GetFirestoreDbInstance();
            var cartSnapshot = await db.Collection("users").Document(userUid).Collection("cart").GetSnapshotAsync();
            foreach (var doc in cartSnapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

         [AllowAnonymous]
        [HttpGet]
        public IActionResult Cancel()
        {
            return Content("Payment Cancelled or Failed. Please try again.");
        }

        [AllowAnonymous]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> XenditWebhook()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            Console.WriteLine($"\n🔔 RAW PAYLOAD RECEIVED: {json}\n");

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                string? orderToken = null;
                string? status = null;
                string? paymentChannel = null;

                string? FindValue(System.Text.Json.JsonElement element, string key) {
                    if (element.TryGetProperty(key, out var val)) return val.GetString();
                    if (element.TryGetProperty("data", out var data) && data.TryGetProperty(key, out var val2)) return val2.GetString();
                    return null;
                }

                orderToken = FindValue(root, "external_id");
                status = FindValue(root, "status");
                paymentChannel = FindValue(root, "payment_channel");

                Console.WriteLine($"Extracted -> Token: {orderToken ?? "NOT FOUND"} | Status: {status ?? "NOT FOUND"} | Channel: {paymentChannel ?? "NOT FOUND"}");

                if (!string.IsNullOrEmpty(orderToken) && status == "PAID")
                {
                    // 🔥 FIX: Pass the dynamic paymentChannel instead of the hardcoded "Online" string!
                    await _orderService.UpdateOrderStatusAsync(orderToken, "Paid", paymentChannel ?? "Online");
                    Console.WriteLine("Order verified as PAID with mapped channel.");
                    return Ok();
                }

                return Ok("Webhook received, but order not marked PAID.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Parsing Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}