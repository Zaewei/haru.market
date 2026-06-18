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

                        string GetVal(string key1, string key2, string fallback) 
                        {
                            if (data.ContainsKey(key1)) return data[key1].ToString()!;
                            if (data.ContainsKey(key2)) return data[key2].ToString()!;
                            return fallback;
                        }

                    usersList.Add(new AdminUserViewModel
                    {
                        Id = document.Id,
                        // Check both conventions
                        Name = GetVal("username", "FullName", "Unknown User"),
                        Email = GetVal("email", "Email", "No Email Provided"),
                        Role = GetVal("role", "Role", "Customer"),
                        Status = GetVal("status", "Status", "Active"),
                        
                        // Dates are a bit stricter, we just check which one exists
                        JoinedAt = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts1 ? ts1.ToDateTime().ToLocalTime() :
                                data.ContainsKey("JoinedAt") && data["JoinedAt"] is Timestamp ts2 ? ts2.ToDateTime().ToLocalTime() : DateTime.Now,
                        
                        LastActive = data.ContainsKey("lastActive") && data["lastActive"] is Timestamp laTs ? laTs.ToDateTime().ToLocalTime() : DateTime.Now,
                        
                        Phone = GetVal("phone", "ContactDetails", "N/A"),
                        Address = GetVal("address", "DeliveryAddress", "No address on file")
                    });
                }
            }
            return usersList;
        }
    }
}