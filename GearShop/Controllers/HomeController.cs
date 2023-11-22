using GearShop.Contracts;
using GearShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GearShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGearShopRepository _gearShopRepository;

        public HomeController(ILogger<HomeController> logger, IGearShopRepository gearShopRepository)
        {
	        _logger = logger;
	        _gearShopRepository = gearShopRepository;
        }

        public async Task<IActionResult> Index()
        {
	        var slaiderData = await _gearShopRepository.MainPageSlaiderDataAsync();
	        ViewData["SlaiderData"] = slaiderData; 
            return View();
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
}