using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using haru.market.Models;

namespace haru.market.Services
{
    // us 12 lookbook service
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

        // writing into the lookbook collection
        public async Task<string> AddLookbookAsync(LookbookViewModel lookbook)
        {
            CollectionReference collection = _firestoreDb.Collection("lookbooks");

            var lookbookData = new Dictionary<string, object>
            {
                { "themeTitle", lookbook.ThemeTitle },
                { "description", lookbook.Description },
                { "mediaUrl", lookbook.MediaUrl },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };

            DocumentReference docRef = await collection.AddAsync(lookbookData);
            return docRef.Id;
        }

        // retrieving the lookbooks
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

            lookbooksList.Add(new LookbookViewModel
            {
                Id = document.Id,
                ThemeTitle = data.ContainsKey("themeTitle") ? data["themeTitle"].ToString()! : "Untitled",
                Description = data.ContainsKey("description") ? data["description"].ToString()! : "",
                MediaUrl = data.ContainsKey("mediaUrl") ? data["mediaUrl"].ToString()! : "placeholder.png",
                
                Views = data.ContainsKey("views") ? Convert.ToInt32(data["views"]) : 0,

                CreatedAt = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts
                    ? ts.ToDateTime()
                    : DateTime.UtcNow
            });
        }
    }

    return lookbooksList;
}
        public async Task SaveToWishlistAsync(string uid, LookbookViewModel lookbook)
        {
            DocumentReference doc = _firestoreDb.Collection("users").Document(uid).Collection("wishlist").Document(lookbook.Id);

            var wishlistData = new Dictionary<string, object>
            {
                { "lookbookId", lookbook.Id ?? "" },
                { "themeTitle", lookbook.ThemeTitle },
                { "description", lookbook.Description },
                { "mediaUrl", lookbook.MediaUrl },
                { "savedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };

            await doc.SetAsync(wishlistData);
        }

        // retrieving a single lookbook by its ID
        public async Task<LookbookViewModel?> GetLookbookAsync(string lookbookId)
        {
            DocumentSnapshot doc = await _firestoreDb.Collection("lookbooks").Document(lookbookId).GetSnapshotAsync();

            if (!doc.Exists)
                return null;

            var data = doc.ToDictionary();

            return new LookbookViewModel
            {
                Id = doc.Id,
                ThemeTitle = data["themeTitle"]?.ToString() ?? "",
                Description = data["description"]?.ToString() ?? "",
                MediaUrl = data["mediaUrl"]?.ToString() ?? "",
            };
        }

        // US-15 retrieving the wishlist for a specific user
        public async Task<List<WishlistItemViewModel>>GetWishlistAsync(string uid)
        {
            var wishlist = new List<WishlistItemViewModel>();

            QuerySnapshot snapshot =await _firestoreDb.Collection("users").Document(uid).Collection("wishlist").GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                wishlist.Add(new WishlistItemViewModel
                {
                    LookbookId = data["lookbookId"]?.ToString() ?? "",
                    ThemeTitle = data["themeTitle"]?.ToString() ?? "",
                    Description = data["description"]?.ToString() ?? "",
                    MediaUrl = data["mediaUrl"]?.ToString() ?? ""
                });
            }

            return wishlist;
        }

        public async Task<int> GetTotalLookbooksCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("lookbooks");
                AggregateQuerySnapshot snapshot = await collection.Count().GetSnapshotAsync();
                return (int)(snapshot.Count ?? 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching lookbook aggregates: {ex.Message}");
                return 0;
            }
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching total lookbook views: {ex.Message}");
                return 0;
            }
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
            catch (Exception)
            {
                // fallback catch loop
            }

            // Default fallback list if database is unreachable
            return new List<string> { "/images/banners/default-hero.jpg" };
        }
    }
}
