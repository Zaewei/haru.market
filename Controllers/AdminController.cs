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

        public AdminController(
            ProductService productService, 
            LookbookService lookbookService, 
            OrderService orderService)
        {
            _productService = productService;
            _lookbookService = lookbookService;
            _orderService = orderService;
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
        public IActionResult Users()
        {
            return View();
        }
    }
}