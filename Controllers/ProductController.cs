using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;
using Microsoft.AspNetCore.Http;

namespace haru.market.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        // project service injection
        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            // if validation fails like missing fields or invalid formats, we return the form with error messages
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // send the validated product package to firebase and get that unique document id back for confirmation
                string generatedDocId = await _productService.AddProductAsync(model);

                // prints out the success message with the unique firebase id
                return Content($"Success! Product added to Firestore catalog with Document ID: {generatedDocId}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to create product: {ex.Message}");
                return View(model);
            }
        }

        // gets the product
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // grab the products
            List<ProductViewModel> catalog = await _productService.GetAllProductsAsync();
            
            // give it to the view to display
            return View(catalog);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId)
        {
            string? uid = HttpContext.Session.GetString("UserUid");

            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _productService.GetProductAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            await _productService.AddToCartAsync(uid, product);

            return RedirectToAction(nameof(Index));
        }
    }
}
