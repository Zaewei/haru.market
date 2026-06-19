using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using haru.market.Models;

namespace haru.market.Services
{
    // Manages the favorites firestore collection.
    // Structure: favorites/{uid}  >  { itemIds: ["productId1", "productId2", ...] }
    public class FavoritesService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ProductService _productService;

        public FavoritesService(FirestoreDb firestoreDb, ProductService productService)
        {
            _firestoreDb = firestoreDb;
            _productService = productService;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private DocumentReference UserDoc(string uid) =>
            _firestoreDb.Collection("favorites").Document(uid);

        // Returns the list of favorited product IDs for a user.
        private async Task<List<string>> GetItemIdsAsync(string uid)
        {
            var snap = await UserDoc(uid).GetSnapshotAsync();
            if (!snap.Exists || !snap.ContainsField("itemIds"))
                return new List<string>();

            var raw = snap.GetValue<List<object>>("itemIds");
            return raw?.Select(x => x.ToString()!).ToList() ?? new List<string>();
        }

        // ── public API ───────────────────────────────────────────────────────

        // Adds productId to the user's favorites if not present,
        // or removes it if already there.  Returns true when the item
        // is now a favorite (i.e. it was just added).
        public async Task<bool> ToggleFavoriteAsync(string uid, string productId)
        {
            var ids = await GetItemIdsAsync(uid);

            if (ids.Contains(productId))
            {
                // Remove
                ids.Remove(productId);
                await UserDoc(uid).SetAsync(new Dictionary<string, object>
                {
                    { "itemIds", ids }
                });
                return false;
            }
            else
            {
                // Add
                ids.Add(productId);
                await UserDoc(uid).SetAsync(new Dictionary<string, object>
                {
                    { "itemIds", ids }
                });
                return true;
            }
        }

        // Returns true when the product is currently in the user's favorites.
        public async Task<bool> IsFavoritedAsync(string uid, string productId)
        {
            var ids = await GetItemIdsAsync(uid);
            return ids.Contains(productId);
        }

        // Fetches full product details for every ID saved in the user's favorites.
        // Products that have been deleted from the catalog are silently skipped.
        public async Task<List<FavoriteItemViewModel>> GetFavoriteItemsAsync(string uid)
        {
            var ids = await GetItemIdsAsync(uid);
            if (!ids.Any())
                return new List<FavoriteItemViewModel>();

            var result = new List<FavoriteItemViewModel>();
            foreach (var id in ids)
            {
                try
                {
                    var product = await _productService.GetProductAsync(id);
                    if (product == null) continue;

                    result.Add(new FavoriteItemViewModel
                    {
                        ProductId  = product.Id ?? id,
                        ProductName = product.Name,
                        Price      = product.Price,
                        ImageUrl   = product.ImageUrl,
                        Color      = product.Color,
                        GroupKey   = product.GroupKey
                    });
                }
                catch
                {
                    // Skip stale / deleted products
                }
            }
            return result;
        }
    }
}
