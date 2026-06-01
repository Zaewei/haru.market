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

        // GET: /Account/Register
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
    }
}