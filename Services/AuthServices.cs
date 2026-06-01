using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

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
                Disabled = false,
            };

            return await FirebaseAuth.DefaultInstance.CreateUserAsync(args);
        }

        // us 03 for login persistence
        public async Task<string> CreateSessionTokenAsync(string uid)
        {
            return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
            }
        }
    }