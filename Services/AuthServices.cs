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
        public async Task<UserRecord> RegisterUserAsync(string email, string password, string username)
        {
            var args = new UserRecordArgs()
            {
                Email = email,
                Password = password,
                DisplayName = username,
                Disabled = false
            };

            UserRecord user = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

            DocumentReference userDoc =
                _firestoreDb
                    .Collection("users")
                    .Document(user.Uid);

            var firestoreUser = new Dictionary<string, object>
            {
                { "uid", user.Uid },
                { "email", email },
                { "username", username },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            await userDoc.SetAsync(firestoreUser);

            return user;
        }

        // us 03 for login persistence
        public async Task<string> CreateSessionTokenAsync(string uid)
        {
            return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
        }

        private readonly FirestoreDb _firestoreDb;

        public AuthService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // test login with firebase auth REST API
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
