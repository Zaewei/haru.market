using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using haru.market.Services;
using haru.market.Models;

namespace haru.market.Controllers
{
    public class Item_DetailsController : Controller
    {
        private readonly ProductService _productService;

        public Item_DetailsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Shop", "Home");
            }

            try
            {
                var product = await _productService.GetProductAsync(id);
                
                if (product == null)
                {
                    return RedirectToAction("Shop", "Home");
                }
                var allProducts = await _productService.GetAllProductsAsync(); 
                
                var variations = allProducts
                    .Where(p => !string.IsNullOrEmpty(p.GroupKey) && 
                                p.GroupKey.Equals(product.GroupKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!variations.Any())
                {
                    variations.Add(product);
                }

                ViewData["Variants"] = variations;

                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Item Details routing error logs: {ex.Message}");
                return RedirectToAction("Shop", "Home");
            }
        }
    }
}