using GearShop.Contracts;
using GearShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.Shop
{
	public class ContactsController : Controller
	{
		private readonly IGearShopRepository _gearShopRepository;

		public ContactsController(IGearShopRepository gearShopRepository)
		{
			_gearShopRepository = gearShopRepository;
		}

		public async Task<IActionResult> Index()
		{
			PageViewModel model = new PageViewModel();
			var dto = await _gearShopRepository.GetPageContent("ContactsPage");
			model.Id = dto.Id;
			model.PageContent = dto.Content;
			return View(model);
		}
	}
}
