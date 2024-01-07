using GearShop.Contracts;
using GearShop.Models.Dto.Products;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.ContentModel;
using System.Collections.Generic;
using GearShop.Helpers;
using GearShop.Models;
using GearShop.Models.Entities;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace GearShop.Controllers.Shop
{
	public class OrderController : Controller
	{
		private readonly IGearShopRepository _repository;
		private readonly INotifier _notifier;

		public OrderController(IGearShopRepository repository, INotifier notifier)
		{
			_repository = repository;
			_notifier = notifier;
		}

		/// <summary>
		/// Страница создания заказа.
		/// </summary>
		/// <param name="json"></param>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		public IActionResult CreateOrder(string json, string userGuid)
		{
			List<ProductDto> model = JsonConvert.DeserializeObject<List<ProductDto>>(json);
			//Перенести в сервис.
            ViewBag.TotalAmount = model.Sum(x => x.Amount * x.Cost);

            return View(model);
		}

		/// <summary>
		/// Создает заказ.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> CreateOrder(List<ProductDto> model, OrderInfo orderInfo, string userGuid)
		{
			string remoteIpAddress = HttpHelper.GetRemoteIp(HttpContext);

			long orderNumber = await _repository.CreateOrder(model, orderInfo, userGuid, remoteIpAddress);
			if (orderNumber == -1)
			{
				return BadRequest();
			}

			_notifier.NewOrder(model, orderInfo, orderNumber); //Уведомляем о новом заказе.
			return Ok(orderNumber);
		}

		/// <summary>
		/// Страница с информацие об успешном создании заказа.
		/// </summary>
		/// <returns></returns>
		public IActionResult SuccessfulСreation(int orderId)
		{
			return View(orderId);
		}

		/// <summary>
		/// Cтраница списка заказов.
		/// </summary>
		/// <returns></returns>
		public IActionResult OrderListPage()
		{
			return View();
		}

		/// <summary>
		/// Cтраница списка заказов.
		/// </summary>
		/// <returns></returns>
		[Authorize(Roles = "Admin")]
		public IActionResult OrderList()
		{
			return View();
		}

		/// <summary>
		/// Получает список заказов.
		/// </summary>
		/// <returns></returns>
		[Authorize(Roles = "Admin")]
		public IActionResult GetOrderList()
		{
			var settings = new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};

			List<OrderDto> orderList = _repository.GetOrderList("").Result;

			var json = JsonConvert.SerializeObject(orderList, Formatting.Indented, settings);
			return Ok(json);
		}
	}
}
