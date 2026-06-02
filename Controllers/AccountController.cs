using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;

namespace haru.market.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService = new AuthService();

        // us 01 customer registration
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var firebaseUser = await _authService.RegisterUserAsync(model.Email, model.Password, model.Username);
                return Content($"Success! User account created with Firebase UID: {firebaseUser.Uid}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                return View(model);
            }
        }

        // us 03 customer login
    
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                return View(model); 
            }

            try
            {
                // using the unique uid from firebase to create a session token
                string mockUid = "J3ZInHktIaS3Lpphm9YUK89RBwU2"; 
                
                string sessionToken = await _authService.CreateSessionTokenAsync(mockUid);
                
                if (model.RememberMe)
                {
                    // browser cookies for remember me
                }

                // printing of success token
                return Content($"Success! Secure Session Token Generated: {sessionToken.Substring(0, 15)}...");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
                return View(model);
            }
        }
    } // closes the accountcontroller class
} // closes the namespace