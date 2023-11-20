using GearShop.Models.Dto.Products;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.ContentModel;

namespace GearShop.Controllers.Shop
{
	public class OrderController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult CreateOrder(string json)
		{
			List<ProductDto> model = JsonConvert.DeserializeObject<List<ProductDto>>(json);

			//Перенести в сервис.
            ViewBag.TotalAmount = model.Sum(x => x.Amount * x.Cost);

            return View(model);
		}

		[HttpPost]
		public IActionResult CreateOrder(List<ProductDto> model1)
		{
			//List<ProductDto> model = JsonConvert.DeserializeObject<List<ProductDto>>(json);

			////Перенести в сервис.
			//ViewBag.TotalAmount = model.Sum(x => x.Amount * x.Cost);

			return Ok(15252);
		}
	}
}
