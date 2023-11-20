using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.Shop
{
	/// <summary>
	/// Корзина.
	/// </summary>
	public class BasketController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Оформить заказ.
		/// </summary>
		/// <returns></returns>
		public IActionResult CreateOrder()
		{
			return RedirectToAction("CreateOrder", "Order");
		}
	}
}
