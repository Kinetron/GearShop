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
			ContactsPageViewModel model = new ContactsPageViewModel();
			model.PageContent = await _gearShopRepository.GetPageContent("ContactsPage");
			return View(model);
		}
	}
}
