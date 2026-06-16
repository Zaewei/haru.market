using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _authService.RegisterUserAsync(
                    model.Email, 
                    model.Password, 
                    model.FullName, 
                    model.ContactDetails, 
                    model.DeliveryAddress
                );

                return RedirectToAction("Login", "Account");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Registration aborted: {ex.Message}");
                return View(model);
            }
        }

        // us 03 customer login - GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // us 03 customer login - POST
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