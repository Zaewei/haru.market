using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using haru.market.Services;
using haru.market.Models;
using System.Collections.Generic; 
using System.Linq; 

namespace haru.market.Controllers
{
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
        public IActionResult Lookbooks()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            var sortedUsers = users.OrderByDescending(u => u.JoinedAt).ToList();
            return View(sortedUsers);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, string status)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new { success = false, message = "Invalid order parameters passed." });
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status, string.Empty);
                return Json(new { success = true, message = "Firestore updated cleanly!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}