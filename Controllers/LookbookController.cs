using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;
using System.Threading.Tasks;

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
            return View(lookbooks);
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
    }
}