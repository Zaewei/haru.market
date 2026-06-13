using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;

namespace haru.market.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

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
                // Attempt to log in the user and get their UID
                string? uid = await _authService.LoginUserAsync(model.Email, model.Password);

                if (string.IsNullOrEmpty(uid))
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password");
                    return View(model);
                }

                HttpContext.Session.SetString("UserUid", uid);

                string sessionToken = await _authService.CreateSessionTokenAsync(uid);
                
                if (model.RememberMe)
                {
                    // browser cookies for remember me
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
                return View(model);
            }
        }
    } // closes the accountcontroller class
} // closes the namespace
