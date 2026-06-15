using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace haru.market.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public CheckoutController(OrderService orderService, ProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        // checkout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // pull first live item from product service for checkout scenario (change this later to reflect actual shopping items)
            var products = await _productService.GetAllProductsAsync();
            var testProduct = products.FirstOrDefault();

            var checkoutSetup = new OrderPlacementViewModel
            {
                CustomerName = string.Empty,
                CustomerEmail = string.Empty,
                ShippingAddress = string.Empty,
                Items = new List<OrderItemModel>()
            };

            if (testProduct != null)
            {
                checkoutSetup.Items.Add(new OrderItemModel
                {
                    ProductId = testProduct.Id ?? "SimulatedId",
                    ProductName = testProduct.Name,
                    Price = testProduct.Price,
                    Quantity = 0
                });
            }

            return View(checkoutSetup);
        }

        // checkout process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(OrderPlacementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // process checkout
                string orderToken = await _orderService.CreateOrderAsync(model);

                // generate xendit payment link
                string xenditCheckoutUrl = await _orderService.CreateXenditInvoiceAsync(orderToken, model);

                // background invoice sent via resend
                _orderService.DispatchInvoiceBackground(orderToken, model);

                // redirect user to xendit transaction page
                return Redirect(xenditCheckoutUrl);
            }
            catch (System.Exception ex)
            {
                // catches any exceptions thrown during the transaction
                return Content($"Checkout Aborted by Transaction Protection: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            // xendit will send the user here after a successful payment
            return Content("Payment Successful! Thank you for your order. Your receipt has been sent to your email.");
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            // xendit will send the user here if they close the payment window
            return Content("Payment Cancelled or Failed. Please try again.");
        }
    }
}