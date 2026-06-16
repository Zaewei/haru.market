using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using haru.market.Models;

namespace haru.market.Services
{
    public class UserService
    {
        private readonly FirestoreDb _firestoreDb;

        public UserService()
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

        public async Task<List<AdminUserViewModel>> GetAllUsersAsync()
        {
            var usersList = new List<AdminUserViewModel>();
            CollectionReference collection = _firestoreDb.Collection("users");
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> data = document.ToDictionary();

                    usersList.Add(new AdminUserViewModel
                    {
                        Id = document.Id,
                        Name = data.ContainsKey("username") ? data["username"].ToString()! : "Unknown User",
                        Email = data.ContainsKey("email") ? data["email"].ToString()! : "No Email Provided",
                        Role = data.ContainsKey("role") ? data["role"].ToString()! : "Customer",
                        Status = data.ContainsKey("status") ? data["status"].ToString()! : "Active",
                        JoinedAt = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp joinTs 
                            ? joinTs.ToDateTime().ToLocalTime() 
                            : DateTime.Now,
                        LastActive = data.ContainsKey("lastActive") && data["lastActive"] is Timestamp activeTs 
                            ? activeTs.ToDateTime().ToLocalTime() 
                            : DateTime.Now,
                        Phone = data.ContainsKey("phone") ? data["phone"].ToString()! : "N/A",
                        Address = data.ContainsKey("address") ? data["address"].ToString()! : "No address on file"
                    });
                }
            }
            return usersList;
        }
    }
}