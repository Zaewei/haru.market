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
                    var data = document.ToDictionary();

                    lookbooksList.Add(new LookbookViewModel
                    {
                        Id = document.Id,
                        ThemeTitle = data.ContainsKey("themeTitle") && data["themeTitle"] != null 
                            ? data["themeTitle"].ToString()! 
                            : "Untitled Campaign",
                            
                        Description = data.ContainsKey("description") && data["description"] != null 
                            ? data["description"].ToString()! 
                            : "",
                            
                        MediaUrl = data.ContainsKey("mediaUrl") && data["mediaUrl"] != null 
                            ? data["mediaUrl"].ToString()! 
                            : "placeholder.png"
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
    }
}
