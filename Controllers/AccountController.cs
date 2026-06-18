using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using haru.market.Models;
using haru.market.Services;
using Google.Cloud.Firestore;
using FirebaseAdmin.Auth;

namespace haru.market.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService; 

        public AccountController(AuthService authService, ProductService productService)
        {
            _authService = authService;
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try 
            {
                string? userUid = await _authService.LoginUserAsync(model.Email, model.Password);

                if (!string.IsNullOrEmpty(userUid))
                {
                    HttpContext.Session.SetString("UserUid", userUid);
                    string userRole = await _productService.GetUserRoleAsync(model.Email);
                    HttpContext.Session.SetString("UserRole", userRole);

                    var identityClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.NameIdentifier, userUid),
                        new Claim(ClaimTypes.Role, userRole)
                    };

                    var claimsIdentity = new ClaimsIdentity(identityClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authenticationProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authenticationProperties
                    );

                    if (userRole == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Login error: {ex.Message}");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials detected.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            // if standard aspnet doesn't work uncomment below to replace
            // await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(haru.market.Models.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userArgs = new FirebaseAdmin.Auth.UserRecordArgs
                {
                    Email = model.Email,
                    Password = model.Password,
                    DisplayName = model.FullName
                };
                var userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);

                var db = Google.Cloud.Firestore.FirestoreDb.Create("haru-market"); 
                var docRef = db.Collection("users").Document(userRecord.Uid);
                
                var userData = new Dictionary<string, object>
                {
                    { "FullName", model.FullName },
                    { "Email", model.Email },
                    { "DeliveryAddress", model.DeliveryAddress },
                    { "ContactDetails", model.ContactDetails },
                    { "Role", "customer" },
                    { "Status", "active" },
                    { "JoinedAt", Google.Cloud.Firestore.Timestamp.GetCurrentTimestamp() }
                };
                
                await docRef.SetAsync(userData);

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Registration failed: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProfile()
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid)) 
            {
                return Unauthorized("User ID not found in claims.");
            }

            var db = Google.Cloud.Firestore.FirestoreDb.Create("haru-market");
            var docRef = db.Collection("users").Document(uid);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return Json(new {
                    fullName = snapshot.GetValue<string>("FullName"),
                    address = snapshot.GetValue<string>("DeliveryAddress"),
                    contact = snapshot.GetValue<string>("ContactDetails")
                });
            }
            return NotFound("User document not found.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string deliveryAddress, string contactDetails)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            var db = Google.Cloud.Firestore.FirestoreDb.Create("haru-market");
            var docRef = db.Collection("users").Document(uid);

            var updates = new Dictionary<string, object>
            {
                { "FullName", fullName },
                { "DeliveryAddress", deliveryAddress },
                { "ContactDetails", contactDetails }
            };

            await docRef.UpdateAsync(updates);

            return Json(new { success = true });
        }
    }
}