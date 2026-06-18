using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace haru.market.Controllers
{
    public class LookbookController : Controller
    {
        private readonly LookbookService _lookbookService;

        public LookbookController(LookbookService lookbookService)
        {
            _lookbookService = lookbookService;
        }

        // lookbook main page
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lookbooks = await _lookbookService.GetAllLookbooksAsync();
            ViewBag.HeroBannerUrls = await _lookbookService.GetShuffledHeroBannersAsync();
            
            return View("~/Views/Home/Lookbook.cshtml", lookbooks); 
        }

        // lookbook upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLookbook(LookbookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var lookbooks = await _lookbookService.GetAllLookbooksAsync();
                return View("Index", lookbooks); // return validation errors if model is invalid
            }

            // publish to repository and get the generated document id
            await _lookbookService.AddLookbookAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // US-15 save lookbook to wishlist
        [HttpPost]
        public async Task<IActionResult> SaveToWishlist(string lookbookId)
        {
            string? uid = HttpContext.Session.GetString("UserUid");

            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("Login", "Account");
            }

            var lookbook = await _lookbookService.GetLookbookAsync(lookbookId);

            if (lookbook == null)
            {
                return NotFound();
            }

            bool alreadyWishlisted = await _lookbookService.IsWishlistedAsync(uid, lookbookId);

            if (alreadyWishlisted)
            {
                await _lookbookService.RemoveFromWishlistAsync(uid, lookbookId);
            }
            else
            {
                await _lookbookService.SaveToWishlistAsync(uid, lookbook);
            }

            return RedirectToAction("Lookbook", "Home");
        }

        // wishlist page
        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            string? uid = HttpContext.Session.GetString("UserUid");

            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("Login","Account");
            }

            var wishlist = await _lookbookService.GetWishlistAsync(uid);

            return View(wishlist);
        }
    }
}
