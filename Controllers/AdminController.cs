using Microsoft.AspNetCore.Mvc;

namespace haru.market.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult Lookbooks() => View();
        public IActionResult ProductManagement() => View();
        public IActionResult Orders() => View();
        public IActionResult Users() => View();
    }
}