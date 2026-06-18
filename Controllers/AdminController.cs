using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using haru.market.Services;
using haru.market.Models;
using System.Collections.Generic;
using System.Linq;

namespace haru.market.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly ProductService _productService;
        private readonly LookbookService _lookbookService;
        private readonly OrderService _orderService;
        private readonly UserService _userService;

        public AdminController(
            ProductService productService,
            LookbookService lookbookService,
            OrderService orderService,
            UserService userService)
        {
            _productService = productService;
            _lookbookService = lookbookService;
            _orderService = orderService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int productsCount = await _productService.GetTotalProductsCountAsync();
            int usersCount = await _productService.GetTotalUsersCountAsync();
            int lookbooksCount = await _lookbookService.GetTotalLookbooksCountAsync();

            int liveLookbookViews = await _lookbookService.GetTotalLookbookViewsCountAsync();
            int liveProductViews = await _productService.GetTotalProductViewsCountAsync();

            int combinedViewsCount = liveLookbookViews + liveProductViews;

            List<LookbookViewModel> lookbooksList = await _lookbookService.GetAllLookbooksAsync();

            List<ProductViewModel> productsList = await _productService.GetAllProductsAsync();

            var topLookbooksList = lookbooksList
                .OrderByDescending(lookbook => lookbook.ThemeTitle)
                .Take(3)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                TotalProducts = productsCount,
                TotalUsers = usersCount,
                TotalLookbooks = lookbooksCount,
                TotalViews = combinedViewsCount,
                RecentLookbooks = lookbooksList,
                RecentProducts = productsList
            };

            ViewData["TopLookbooks"] = topLookbooksList;
            ViewData["ChartLabels"] = lookbooksList.Select(l => l.ThemeTitle).ToArray();
            ViewData["ChartData"] = lookbooksList.Select(l => l.Views).ToArray();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var liveOrders = await _orderService.GetAllOrdersAsync();

            return View(liveOrders);
        }

       [HttpGet]
        public async Task<IActionResult> Lookbooks()
        {
            var lookbooks = await _lookbookService.GetAllLookbooksAsync();
            var ordered = lookbooks.OrderByDescending(l => l.IsFeatured).ToList();
            return View(ordered);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFeatured(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid campaign tracking entry identification identifier." });
            }

            try
            {
                await _lookbookService.SetFeaturedLookbookAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLookbook(LookbookViewModel lookbook)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please check all fields and fill them out correctly." });
            }

            try
            {
                string newId = await _lookbookService.AddLookbookAsync(lookbook);
                return Json(new { 
                    success = true, 
                    id = newId, 
                    themeTitle = lookbook.ThemeTitle, 
                    description = lookbook.Description, 
                    mediaUrl = lookbook.MediaUrl 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLookbook(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid campaign tracking entry identification identifier." });
            }

            try
            {
                await _lookbookService.DeleteLookbookAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            var sortedUsers = users.OrderByDescending(u => u.JoinedAt).ToList();
            return View(sortedUsers);
        }

  
        [HttpGet]
        public async Task<IActionResult> ProductManagement()
        {
            try
            {
                List<ProductViewModel> productsList = await _productService.GetAllProductsAsync();
                return View(productsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProductManagement error: {ex.Message}");
                return View(new List<ProductViewModel>());
            }
        }

     
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel product)
        {
            if (!ModelState.IsValid)
            {
                TempData["PMError"] = "Please check the product details and try again.";
                return RedirectToAction("ProductManagement");
            }

            await _productService.AddProductAsync(product);
            return RedirectToAction("ProductManagement");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, string status, string? paymentChannel = null)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new { success = false, message = "Invalid order parameters passed." });
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status, paymentChannel ?? "");
                return Json(new { success = true, message = "Firestore updated cleanly!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            try 
            {
                await _productService.GetFirestoreDbInstance()
                    .Collection("orders")
                    .Document(orderId)
                    .DeleteAsync();
                    
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStats(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required.");
            
            var stats = await _orderService.GetUserActivityAsync(email);
            return Json(stats);
        }
    }
}
