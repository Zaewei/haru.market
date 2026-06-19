using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using haru.market.Models;
using System.Linq;

namespace haru.market.Services
{
    public class LookbookService
    {
        private readonly FirestoreDb _firestoreDb;

        public LookbookService()
        {
            string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";
            var credential = Google.Apis.Auth.OAuth2.ServiceAccountCredential.FromServiceAccountData(
                new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(keyPath)))
            ).ToGoogleCredential();

            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = "haru-market",
                Credential = credential
            }.Build();
        }
        public async Task<string> AddLookbookAsync(LookbookViewModel lookbook)
        {
            CollectionReference collection = _firestoreDb.Collection("lookbooks");

            var lookbookData = new Dictionary<string, object>
            {
                { "themeTitle", lookbook.ThemeTitle },
                { "description", lookbook.Description },
                { "views", 0 },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "mediaUrl", lookbook.MediaUrl ?? "" },
                { "mediaUrls", new List<string> { lookbook.MediaUrl ?? "" } },
                { "isFeatured", false }
            };

            DocumentReference docRef = await collection.AddAsync(lookbookData);
            return docRef.Id;
        }

        public async Task<List<LookbookViewModel>> GetAllLookbooksAsync()
        {
            var lookbooksList = new List<LookbookViewModel>();
            CollectionReference collection = _firestoreDb.Collection("lookbooks");
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> data = document.ToDictionary();
                    var lookbook = new LookbookViewModel
                    {
                        Id = document.Id,
                        ThemeTitle = data.ContainsKey("themeTitle") ? data["themeTitle"].ToString()! : "Untitled",
                        Description = data.ContainsKey("description") ? data["description"].ToString()! : "",
                        Views = data.ContainsKey("views") ? Convert.ToInt32(data["views"]) : 0,
                        CreatedAt = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts ? ts.ToDateTime() : DateTime.UtcNow,
                        MediaUrl = "",
                        
                        IsFeatured = data.ContainsKey("isFeatured") && Convert.ToBoolean(data["isFeatured"])
                    };

                    if (data.ContainsKey("mediaUrl"))
                    {
                        lookbook.MediaUrl = data["mediaUrl"].ToString()!;
                    }
                    else if (data.ContainsKey("mediaUrls") && data["mediaUrls"] is List<object> arrayList && arrayList.Any())
                    {
                        lookbook.MediaUrl = arrayList.First().ToString()!;
                    }

                    lookbooksList.Add(lookbook);
                }
            }

            return lookbooksList;
        }

        public async Task<LookbookViewModel?> GetLookbookAsync(string lookbookId)
        {
            DocumentSnapshot doc = await _firestoreDb.Collection("lookbooks").Document(lookbookId).GetSnapshotAsync();

            if (!doc.Exists)
                return null;

            var data = doc.ToDictionary();
            var lookbook = new LookbookViewModel
            {
                Id = doc.Id,
                ThemeTitle = data.ContainsKey("themeTitle") ? data["themeTitle"].ToString()! : "",
                Description = data.ContainsKey("description") ? data["description"].ToString()! : "",
                MediaUrl = "",
                
                IsFeatured = data.ContainsKey("isFeatured") && Convert.ToBoolean(data["isFeatured"])
            };

            if (data.ContainsKey("mediaUrl"))
            {
                lookbook.MediaUrl = data["mediaUrl"].ToString()!;
            }
            else if (data.ContainsKey("mediaUrls") && data["mediaUrls"] is List<object> arrayList && arrayList.Any())
            {
                lookbook.MediaUrl = arrayList.First().ToString()!;
            }

            return lookbook;
        }

        public async Task<int> GetTotalLookbooksCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("lookbooks");
                AggregateQuerySnapshot snapshot = await collection.Count().GetSnapshotAsync();
                return (int)(snapshot.Count ?? 0);
            }
            catch (Exception) { return 0; }
        }

        public async Task<int> GetTotalLookbookViewsCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("lookbooks");
                QuerySnapshot snapshot = await collection.GetSnapshotAsync();
                int totalViews = 0;

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists && document.ContainsField("views"))
                    {
                        totalViews += Convert.ToInt32(document.GetValue<object>("views"));
                    }
                }
                return totalViews;
            }
            catch (Exception) { return 0; }
        }

        public async Task<List<string>> GetShuffledHeroBannersAsync()
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection("banners").Document("hero");
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    Dictionary<string, object> data = snapshot.ToDictionary();
                    if (data.ContainsKey("imageurls") && data["imageurls"] is List<object> arrayList)
                    {
                        List<string> images = arrayList.Select(img => img.ToString()!).ToList();
                        if (images.Any())
                        {
                            Random rand = new Random();
                            return images.OrderBy(x => rand.Next()).ToList();
                        }
                    }
                }
            }
            catch (Exception) { }
            return new List<string> { "/images/banners/default-hero.jpg" };
        }
        
        public async Task SaveToWishlistAsync(string userId, LookbookViewModel lookbook)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection("wishlists").Document(userId);
                var data = new Dictionary<string, object>
                {
                    { "itemIds", FieldValue.ArrayUnion(lookbook.Id ?? "") }
                };
                await docRef.SetAsync(data, SetOptions.MergeAll);
            }
            catch (Exception) { }
        }

        public async Task RemoveFromWishlistAsync(string uid, string lookbookId)
        {
            DocumentReference wishlistDoc = _firestoreDb.Collection("wishlists").Document(uid);

            await wishlistDoc.UpdateAsync("itemIds", FieldValue.ArrayRemove(lookbookId));
        }

        public async Task<bool> IsWishlistedAsync(string uid, string lookbookId)
        {
            DocumentSnapshot snapshot = await _firestoreDb.Collection("wishlists").Document(uid).GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            if (!snapshot.ContainsField("itemIds"))
            {
                return false;
            }

            List<string> itemIds = snapshot.GetValue<List<string>>("itemIds");

            return itemIds.Contains(lookbookId);
        }

        public async Task<List<string>> GetWishlistAsync(string userId)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection("wishlists").Document(userId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var data = snapshot.ToDictionary();
                    if (data.ContainsKey("itemIds") && data["itemIds"] is List<object> list)
                    {
                        return list.Select(x => x.ToString()!).ToList();
                    }
                }
            }
            catch (Exception) { }
            return new List<string>();
        }

        public async Task DeleteLookbookAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                await _firestoreDb.Collection("lookbooks").Document(id).DeleteAsync();
            }
        }
        public async Task SetFeaturedLookbookAsync(string id)
        {
            var collection = _firestoreDb.Collection("lookbooks");
            var snapshot = await collection.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    var updateData = new Dictionary<string, object>
                    {
                        { "isFeatured", doc.Id == id }
                    };
                    await doc.Reference.UpdateAsync(updateData);
                }
            }
        }

       public async Task IncrementViewsAsync(string id, string type)
        {
            var collectionName = type == "Lookbook" ? "lookbooks" : "collections";
            var docRef = _firestoreDb.Collection(collectionName).Document(id);
            
            await docRef.UpdateAsync("views", Google.Cloud.Firestore.FieldValue.Increment(1));
        }
    }
}
