using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using haru.market.Services;
using haru.market.Models;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace haru.market.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly ProductService _productService;
        private readonly LookbookService _lookbookService;
        private readonly OrderService _orderService;
        private readonly UserService _userService;

        public AdminController(
            ProductService productService,
            LookbookService lookbookService,
            OrderService orderService,
            UserService userService)
        {
            _productService = productService;
            _lookbookService = lookbookService;
            _orderService = orderService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int productsCount = await _productService.GetTotalProductsCountAsync();
            int usersCount = await _productService.GetTotalUsersCountAsync();
            int lookbooksCount = await _lookbookService.GetTotalLookbooksCountAsync();

            int liveLookbookViews = await _lookbookService.GetTotalLookbookViewsCountAsync();
            int liveProductViews = await _productService.GetTotalProductViewsCountAsync();

            int combinedViewsCount = liveLookbookViews + liveProductViews;

            List<LookbookViewModel> lookbooksList = await _lookbookService.GetAllLookbooksAsync();

            List<ProductViewModel> productsList = await _productService.GetAllProductsAsync();

            var topLookbooksList = lookbooksList
                .OrderByDescending(lookbook => lookbook.ThemeTitle)
                .Take(3)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                TotalProducts = productsCount,
                TotalUsers = usersCount,
                TotalLookbooks = lookbooksCount,
                TotalViews = combinedViewsCount,
                RecentLookbooks = lookbooksList,
                RecentProducts = productsList
            };

            ViewData["TopLookbooks"] = topLookbooksList;
            ViewData["ChartLabels"] = lookbooksList.Select(l => l.ThemeTitle).ToArray();
            ViewData["ChartData"] = lookbooksList.Select(l => l.Views).ToArray();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var liveOrders = await _orderService.GetAllOrdersAsync();

            return View(liveOrders);
        }

       [HttpGet]
        public async Task<IActionResult> Lookbooks()
        {
            var lookbooks = await _lookbookService.GetAllLookbooksAsync();
            var ordered = lookbooks.OrderByDescending(l => l.IsFeatured).ToList();
            return View(ordered);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFeatured(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid campaign tracking entry identification identifier." });
            }

            try
            {
                await _lookbookService.SetFeaturedLookbookAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLookbook(LookbookViewModel lookbook)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please check all fields and fill them out correctly." });
            }

            try
            {
                string newId = await _lookbookService.AddLookbookAsync(lookbook);
                return Json(new { 
                    success = true, 
                    id = newId, 
                    themeTitle = lookbook.ThemeTitle, 
                    description = lookbook.Description, 
                    mediaUrl = lookbook.MediaUrl 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLookbook(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid campaign tracking entry identification identifier." });
            }

            try
            {
                await _lookbookService.DeleteLookbookAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            var sortedUsers = users.OrderByDescending(u => u.JoinedAt).ToList();
            return View(sortedUsers);
        }

  
        [HttpGet]
        public async Task<IActionResult> ProductManagement()
        {
            try
            {
                List<ProductViewModel> productsList = await _productService.GetAllProductsAsync();
                return View(productsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProductManagement error: {ex.Message}");
                return View(new List<ProductViewModel>());
            }
        }

     
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel product)
        {
            if (!ModelState.IsValid)
            {
                TempData["PMError"] = "Please check the product details and try again.";
                return RedirectToAction("ProductManagement");
            }

            await _productService.AddProductAsync(product);
            return RedirectToAction("ProductManagement");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try 
            {
                await _productService.DeleteProductAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, string status, string? paymentChannel = null)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new { success = false, message = "Invalid order parameters passed." });
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status, paymentChannel ?? "");
                return Json(new { success = true, message = "Firestore updated cleanly!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            try 
            {
                await _productService.GetFirestoreDbInstance()
                    .Collection("orders")
                    .Document(orderId)
                    .DeleteAsync();
                    
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStats(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required.");
            
            var stats = await _orderService.GetUserActivityAsync(email);
            return Json(stats);
        }

        public class UpdateProductRequest 
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Color { get; set; } = string.Empty;
            public Dictionary<string, int> StockQuantity { get; set; } = new Dictionary<string, int>();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                return Json(new { success = false, message = "Product ID is missing." });
            }

            try
            {
                await _productService.UpdateProductAsync(
                    request.Id, 
                    request.Name, 
                    request.Price, 
                    request.Color, 
                    request.StockQuantity
                );
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CustomerInsights()
        {
            var users  = await _userService.GetAllUsersAsync();
            var orders = await _orderService.GetAllOrdersAsync();

            var customers = users
                .Where(u => string.Equals(u.Role, "Customer", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var statusDist = customers
                .GroupBy(u => NormalizeStatus(u.Status))
                .ToDictionary(g => g.Key, g => g.Count());

            int activeCount   = statusDist.GetValueOrDefault("Active", 0);
            int inactiveCount = customers.Count - activeCount;

            var orderZones = orders
                .Select(o => ExtractDeliveryZone(o.ShippingAddress))
                .Where(z => z != "Unknown")
                .GroupBy(z => z)
                .ToDictionary(g => g.Key, g => g.Count());

            var userZones = users
                .Select(u => ExtractDeliveryZone(u.Address))
                .Where(z => z != "Unknown")
                .GroupBy(z => z)
                .ToDictionary(g => g.Key, g => g.Count());

            var mergedZones = new Dictionary<string, int>(orderZones);
            foreach (var kv in userZones)
            {
                if (!mergedZones.ContainsKey(kv.Key))
                    mergedZones[kv.Key] = kv.Value;
                else
                    mergedZones[kv.Key] += kv.Value;
            }

            var topZonesRaw = mergedZones
                .OrderByDescending(kv => kv.Value)
                .Take(8)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            int totalZoneCount = topZonesRaw.Values.Sum();

            var topZoneSummaries = topZonesRaw
                .Select(kv => new DeliveryZoneSummary
                {
                    Zone          = kv.Key,
                    OrderCount    = orderZones.GetValueOrDefault(kv.Key, 0),
                    CustomerCount = userZones.GetValueOrDefault(kv.Key, 0),
                    Percentage    = totalZoneCount > 0
                                        ? Math.Round((double)kv.Value / totalZoneCount * 100, 1)
                                        : 0
                })
                .ToList();

            string topZoneLabel = topZoneSummaries.FirstOrDefault()?.Zone ?? "N/A";

            var now = DateTime.Now;
            var trendMonths = Enumerable.Range(0, 6)
                .Select(i => now.AddMonths(-i))
                .Reverse()
                .ToList();

            var registrationTrend = trendMonths.ToDictionary(
                m => m.ToString("MMM yyyy"),
                m => customers.Count(u =>
                    u.JoinedAt.Year  == m.Year &&
                    u.JoinedAt.Month == m.Month)
            );

            var recentCustomers = customers
                .OrderByDescending(u => u.JoinedAt)
                .Take(5)
                .ToList();

            var viewModel = new CustomerInsightViewModel
            {
                TotalCustomers       = users.Count(u => string.Equals(u.Role, "Customer", StringComparison.OrdinalIgnoreCase)),
                ActiveCustomers      = activeCount,
                InactiveCustomers    = inactiveCount,
                UniqueDeliveryZones  = mergedZones.Count,
                TopDeliveryZone      = topZoneLabel,
                DeliveryZoneDistribution = topZonesRaw,
                RegistrationTrend    = registrationTrend,
                StatusDistribution   = statusDist,
                TopDeliveryZones     = topZoneSummaries,
                RecentCustomers      = recentCustomers
            };

            return View(viewModel);
        }

        private static string ExtractDeliveryZone(string rawAddress)
        {
            if (string.IsNullOrWhiteSpace(rawAddress) ||
                rawAddress.Equals("No Address Provided", StringComparison.OrdinalIgnoreCase) ||
                rawAddress.Equals("No address on file", StringComparison.OrdinalIgnoreCase))
            {
                return "Unknown";
            }

            var knownLocations = new[]
            {
                "Quezon City", "Manila", "Makati", "Pasig", "Taguig", "Marikina",
                "Mandaluyong", "Pasay", "Caloocan", "Valenzuela", "Parañaque",
                "Las Piñas", "Muntinlupa", "Malabon", "Navotas", "San Juan",
                "Pateros", "Antipolo", "Cainta", "Taytay",
                "Cebu City", "Cebu", "Davao City", "Davao", "Iloilo City", "Iloilo",
                "Bacolod", "Cagayan de Oro", "CDO", "Zamboanga", "General Santos",
                "GenSan", "Baguio", "Tacloban", "Butuan", "Legazpi", "Naga",
                "Lipa", "Batangas", "Lucena", "Cavite", "Laguna", "Rizal",
                "Bulacan", "Pampanga", "Tarlac", "Pangasinan", "La Union",
                "Ilocos", "Cagayan", "Isabela", "Nueva Ecija", "Benguet",
                "Albay", "Camarines", "Masbate", "Samar", "Leyte",
                "Negros", "Bohol", "Palawan", "Mindoro", "Romblon",
                "Zamboanga", "Lanao", "Cotabato", "Maguindanao", "Sultan Kudarat",
                "Agusan", "Surigao", "Misamis", "Bukidnon",
                "United States", "USA", "Singapore", "Japan", "Australia",
                "United Kingdom", "UAE", "Canada", "Hong Kong", "Saudi Arabia"
            };

            var upper = rawAddress.ToUpperInvariant();
            foreach (var loc in knownLocations)
            {
                if (upper.Contains(loc.ToUpperInvariant()))
                    return loc;
            }

            var parts = rawAddress
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p) && !IsNumeric(p))
                .ToList();

            if (parts.Count >= 2)
                return parts[^2];
            if (parts.Count == 1)
                return parts[0];

            return "Unknown";
        }

        private static bool IsNumeric(string s) =>
            s.All(c => char.IsDigit(c) || c == '-' || c == ' ');

        private static string NormalizeStatus(string raw) =>
            string.IsNullOrWhiteSpace(raw) ? "Unknown"
            : char.ToUpper(raw[0]) + raw[1..].ToLower();

        [HttpPost]
        public async Task<IActionResult> ExportSalesCsv(DateTime startDate, DateTime endDate)
        {
            var orders = await _orderService.GetAllOrdersAsync();

            // Filter orders based on the provided date range
            var filtered = orders.Where(o => o.Date >= startDate && o.Date <= endDate).ToList();

            var csv = new System.Text.StringBuilder();

            csv.AppendLine("OrderId,Customer,TotalAmount,Status");

            foreach (var order in filtered)
            {
                csv.AppendLine($"{order.Id}," + $"{order.Name}," + $"{order.Total:F2}," + $"{order.Status}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"SalesReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpPost]
        public async Task<IActionResult> ExportInventoryCsv()
        {
            var products = await _productService.GetAllProductsAsync();

            var csv = new System.Text.StringBuilder();

            csv.AppendLine("Product,Price,Stock");

            foreach (var product in products)
            {
                csv.AppendLine($"{product.Name}," + $"{product.Price:F2}," + $"{product.TotalStock}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"InventoryReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpPost]
        public async Task<IActionResult> ExportSalesPdf(DateTime startDate, DateTime endDate)
        {
            var orders = await _orderService.GetAllOrdersAsync();

            var filtered = orders.Where(o => o.Date >= startDate && o.Date <= endDate).ToList();

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("Sales Report")
                        .FontSize(20)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Date Range: {startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}");
                        col.Item().PaddingBottom(10);

                        foreach (var order in filtered)
                        {
                            col.Item().Text($"{order.Id} | {order.Name} | ₱{order.Total:F2}");
                        }

                        col.Item().PaddingTop(10);
                        col.Item().Text($"Total Sales: ₱{filtered.Sum(x => x.Total):F2}").Bold();
                    });
                });
            }).GeneratePdf();

            Response.Headers.Append("Content-Disposition", $"attachment; filename=SalesReport_{DateTime.Now:yyyyMMdd}.pdf");
            return File(pdfBytes, "application/pdf");
        }

        [HttpPost]
        public async Task<IActionResult> ExportInventoryPdf()
        {
            var products = await _productService.GetAllProductsAsync();

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("Inventory Report")
                        .FontSize(20)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        foreach (var product in products)
                        {
                            col.Item().Text($"{product.Name} | ₱{product.Price:F2} | Stock: {product.TotalStock}");
                        }

                        col.Item().PaddingTop(10);
                        col.Item().Text($"Total Products: {products.Count}").Bold();
                    });
                });
            }).GeneratePdf();

            Response.Headers.Append("Content-Disposition", $"attachment; filename=InventoryReport_{DateTime.Now:yyyyMMdd}.pdf");
            return File(pdfBytes, "application/pdf");
        }
    }
}
