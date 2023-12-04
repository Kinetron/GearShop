using GearShop.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.Shop
{
	/// <summary>
	/// Статьи.
	/// </summary>
	public class ArticlesController : Controller
	{
		private readonly IGearShopRepository _gearShopRepository;

		public ArticlesController(IGearShopRepository gearShopRepository)
		{
			_gearShopRepository = gearShopRepository;
		}
		public async Task<IActionResult> Index()
		{
			ViewData["MainContent"] = await _gearShopRepository.GetChapterContent(null);
			return View();
		}
	}
}
