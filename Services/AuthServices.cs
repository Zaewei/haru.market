using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace haru.market.Services
{
    public class AuthService
    {
        public async Task<UserRecord> RegisterUserAsync(string email, string password, string username, string phone, string address)
        {
            var args = new UserRecordArgs()
            {
                Email = email,
                Password = password,
                DisplayName = username,
                Disabled = false
            };

            UserRecord user = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

            DocumentReference userDoc = _firestoreDb.Collection("users").Document(user.Uid);

            var firestoreUser = new Dictionary<string, object>
            {
                { "uid", user.Uid },
                { "email", email },
                { "username", username },
                { "role", "Customer" },
                { "status", "active" },
                { "createdAt", Timestamp.GetCurrentTimestamp() },
                { "lastActive", Timestamp.GetCurrentTimestamp() },
                { "phone", phone.Trim() },
                { "address", address.Trim() }
            };

            await userDoc.SetAsync(firestoreUser);

            return user;
        }

        public async Task<string> CreateSessionTokenAsync(string uid)
        {
            return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
        }

        private readonly FirestoreDb _firestoreDb;

        public AuthService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<string?> LoginUserAsync(string email, string password)
        {
            string apiKey = "AIzaSyA9djCxGd4Xg0QJKCm_VIPfYvytg5-CjOQ";

            using var client = new HttpClient();

            var requestBody = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);

            return doc.RootElement.GetProperty("localId").GetString();
        }
    }
}