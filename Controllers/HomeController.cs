using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace haru.market.Controllers
{
    public class HomeViewModel
    {
        public IEnumerable<LookbookViewModel>? Lookbooks { get; set; }
        public IEnumerable<ProductViewModel>? Products { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly LookbookService _lookbookService;

        public HomeController(ProductService productService, LookbookService lookbookService)
        {
            _productService = productService;
            _lookbookService = lookbookService;
        }

        // gets home/index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.HeroBannerUrls = await _lookbookService.GetShuffledHeroBannersAsync();

            var lookbooks = await _lookbookService.GetAllLookbooksAsync();
            var products = await _productService.GetAllProductsAsync();

            var model = new HomeViewModel
            {
                Lookbooks = lookbooks,
                Products = products
            };

            return View(model);
        }

        // gets home/shop
        public async Task<IActionResult> Shop()
        {
            var activeProducts = await _productService.GetAllProductsAsync();
            return View(activeProducts);
        }

        // gets home/lookbook
        [HttpGet]
        public async Task<IActionResult> Lookbook()
        {
            ViewBag.HeroBannerUrls = await _lookbookService.GetShuffledHeroBannersAsync();

            var lookbooks = await _lookbookService.GetAllLookbooksAsync();
            var ordered = lookbooks.OrderByDescending(l => l.IsFeatured).ToList();

            string? uid = HttpContext.Session.GetString("UserUid");

            if (!string.IsNullOrEmpty(uid))
            {
                ViewBag.WishlistIds = await _lookbookService.GetWishlistAsync(uid);
            }
            else
            {
                ViewBag.WishlistIds = new List<string>();
            }

            return View(ordered);
        }
    }
}
