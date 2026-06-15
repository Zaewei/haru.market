using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using haru.market.Models;
using haru.market.Services;

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
                // create their credentials in firebase auth and store their basic info in the database
                var firebaseUser = await _authService.RegisterUserAsync(model.Email, model.Password, model.FullName);
                
                // instant login after sign up
                string? userUid = await _authService.LoginUserAsync(model.Email, model.Password);

                if (!string.IsNullOrEmpty(userUid))
                {
                    // save their uid in session 
                    HttpContext.Session.SetString("UserUid", userUid);

                    //  looks up the user's role in the database (default is "customer")
                    string userRole = await _productService.GetUserRoleAsync(model.Email);

                    // save role metadata in session
                    HttpContext.Session.SetString("UserRole", userRole);

                    // redirect them to the appropriate page based on their role
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
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                return View(model);
            }

            // fallback error
            return RedirectToAction("Login", "Account");
        }

        // us 03 customer login
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
    } // closes the accountcontroller class
} // closes the namespace
