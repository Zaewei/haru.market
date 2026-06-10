using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using System.Collections.Generic;

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
    }
}
