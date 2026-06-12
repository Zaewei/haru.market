using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using haru.market.Models;
using haru.market.Services;
using System.Dynamic;
using System.Threading.Tasks;

namespace haru.market.Controllers;

public class HomeController : Controller
{
    private readonly ProductService _productService;
    private readonly LookbookService _lookbookService;

    // firestore service
    public HomeController(ProductService productService, LookbookService lookbookService)
    {
        _productService = productService;
        _lookbookService = lookbookService;
    }

    // gets home/index
    public async Task<IActionResult> Index()
    {
        // pull active data from firestore
        var activeProducts = await _productService.GetAllProductsAsync();
        var activeLookbooks = await _lookbookService.GetAllLookbooksAsync();

        // renders the view with the data
        dynamic homeViewModel = new ExpandoObject();
        homeViewModel.Products = activeProducts;
        homeViewModel.Lookbooks = activeLookbooks;

        return View(homeViewModel);
    }

    // gets home/shop
    public async Task<IActionResult> Shop()
    {
        // pulls the inventory from firestore
        var activeProducts = await _productService.GetAllProductsAsync();
        return View(activeProducts);
    }

    // gets home/lookbook
    public async Task<IActionResult> Lookbook()
    {
        var activeLookbooks = await _lookbookService.GetAllLookbooksAsync();
        return View(activeLookbooks);
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}