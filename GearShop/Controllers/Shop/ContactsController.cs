using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.Shop
{
	public class ContactsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
