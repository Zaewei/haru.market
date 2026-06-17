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
    }
}