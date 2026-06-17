using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using haru.market.Services;
using haru.market.Models;

namespace haru.market.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ProductService _productService;

        public CartController(ProductService productService)
        {
            _productService = productService;
        }
        
        public class CartSubmissionModel
        {
            public string ProductId { get; set; } = "";
            public string Size { get; set; } = "M";
            public int Quantity { get; set; } = 1;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CartSubmissionModel payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.ProductId))
            {
                return Json(new { success = false, message = "Invalid product data layout parameters." });
            }

            try
            {
                string userUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                 ?? User.Identity?.Name 
                                 ?? "";

                if (string.IsNullOrEmpty(userUid))
                {
                    return Json(new { success = false, message = "User verification session state lost." });
                }

                var targetProduct = await _productService.GetProductAsync(payload.ProductId);
                if (targetProduct == null)
                {
                    return Json(new { success = false, message = "Product document not found inside collection catalogs." });
                }

                targetProduct.Description = $"Size: {payload.Size}";

                await _productService.AddToCartAsync(userUid, targetProduct);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartPartial()
        {
            try
            {
                string userUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                 ?? User.Identity?.Name 
                                 ?? "";

                if (string.IsNullOrEmpty(userUid))
                {
                    return PartialView("~/Views/Shared/Popups/_Cart_Popup_Page.cshtml", new List<CartItemViewModel>());
                }

                var activeCartItems = await _productService.GetCartItemsAsync(userUid);
                return PartialView("~/Views/Shared/Popups/_Cart_Popup_Page.cshtml", activeCartItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cart partial rendering error trace: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQtyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductId)) return Json(new { success = false });

            try
            {
                string userUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "";
                if (string.IsNullOrEmpty(userUid)) return Json(new { success = false });

                var docRef = _productService.GetFirestoreDbInstance().Collection("users").Document(userUid).Collection("cart").Document(request.ProductId);
                await docRef.UpdateAsync("quantity", request.Quantity);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteItem([FromBody] DeleteItemRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductId)) return Json(new { success = false });

            try
            {
                string userUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "";
                if (string.IsNullOrEmpty(userUid)) return Json(new { success = false });

                var docRef = _productService.GetFirestoreDbInstance().Collection("users").Document(userUid).Collection("cart").Document(request.ProductId);
                await docRef.DeleteAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateQtyRequest { public string ProductId { get; set; } = ""; public int Quantity { get; set; } }
        public class DeleteItemRequest { public string ProductId { get; set; } = ""; }
    }
}