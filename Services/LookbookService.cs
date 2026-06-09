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
    }
}