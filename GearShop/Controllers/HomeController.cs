using GearShop.Contracts;
using GearShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using GearShop.Models.ViewModels;
using Newtonsoft.Json;

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
	        PageViewModel model = new PageViewModel();

            var dto = await _gearShopRepository.GetPageContent("MainPage");
            model.Id = dto.Id;
            model.PageContent = dto.Content;
            ViewData["Title"] = dto.Title;// "Главная";

            //For show nesfeed block.
			var articleDto = await _gearShopRepository.GetPageContent("ArticlesPage");
            ViewData["articlePageId"] = articleDto.Id;
          
			return View(model);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Возвращает содержимое footer.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Footer()
        {
	        var dto = await _gearShopRepository.GetPageContent("Footer");
	        if (string.IsNullOrEmpty(dto.Content)) return BadRequest();

			return Ok(JsonConvert.SerializeObject(new {id = dto.Id, text = dto.Content}));
        }

	}
}