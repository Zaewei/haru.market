using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using haru.market.Services;

namespace haru.market.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly FavoritesService _favoritesService;

        public FavoritesController(FavoritesService favoritesService)
        {
            _favoritesService = favoritesService;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private string GetUid() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.Identity?.Name
            ?? "";

        // ── endpoints ────────────────────────────────────────────────────────

        // POST /Favorites/Toggle
        // Body: { "productId": "abc123" }
        // Returns: { success, isFavorited }

        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] FavoriteToggleRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductId))
                return Json(new { success = false, message = "Missing product ID." });

            var uid = GetUid();
            if (string.IsNullOrEmpty(uid))
                return Json(new { success = false, message = "User session expired." });

            try
            {
                bool nowFavorited = await _favoritesService.ToggleFavoriteAsync(uid, request.ProductId);
                return Json(new { success = true, isFavorited = nowFavorited });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // GET /Favorites/IsFavorited?productId=abc123
        // Returns: { isFavorited }

        [HttpGet]
        public async Task<IActionResult> IsFavorited(string productId)
        {
            var uid = GetUid();
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(productId))
                return Json(new { isFavorited = false });

            try
            {
                bool favorited = await _favoritesService.IsFavoritedAsync(uid, productId);
                return Json(new { isFavorited = favorited });
            }
            catch
            {
                return Json(new { isFavorited = false });
            }
        }


        // GET /Favorites/GetFavoritesPartial
        // Returns the rendered _Favorites_Popup_Page partial with live data.

        [HttpGet]
        public async Task<IActionResult> GetFavoritesPartial()
        {
            try
            {
                var uid = GetUid();
                if (string.IsNullOrEmpty(uid))
                    return PartialView("~/Views/Shared/Popups/_Favorites_Popup_Page.cshtml",
                        new System.Collections.Generic.List<haru.market.Models.FavoriteItemViewModel>());

                var items = await _favoritesService.GetFavoriteItemsAsync(uid);
                return PartialView("~/Views/Shared/Popups/_Favorites_Popup_Page.cshtml", items);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favorites partial error: {ex.Message}");
                return BadRequest();
            }
        }

        // ── request model ────────────────────────────────────────────────────

        public class FavoriteToggleRequest
        {
            public string ProductId { get; set; } = "";
        }
    }
}
