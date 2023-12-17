using GearShop.Contracts;
using GearShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using GearShop.Models.Dto;
using GearShop.Services;

namespace GearShop.Controllers.Shop
{
	public class ContactsController : Controller
	{
		private readonly IGearShopRepository _gearShopRepository;
		private readonly INotifier _notifier;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public ContactsController(IGearShopRepository gearShopRepository, INotifier notifier, IHttpContextAccessor httpContextAccessor)
		{
			_gearShopRepository = gearShopRepository;
			_notifier = notifier;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IActionResult> Index()
		{
			PageViewModel model = new PageViewModel();
			var dto = await _gearShopRepository.GetPageContent("ContactsPage");
			model.Id = dto.Id;
			model.PageContent = dto.Content;
			return View(model);
		}

		/// <summary>
		/// Обработка формы «Написать нам».
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> SendMessageToEmail(string data)
		{
			SendToEmailDto dto = JsonConvert.DeserializeObject<SendToEmailDto>(data);

			string remoteIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
			if (Request.Headers.ContainsKey("X-Forwarded-For"))
				remoteIpAddress = Request.Headers["X-Forwarded-For"];

			bool result = await _notifier.SendMessageToManagerAsync(dto.Name, dto.Email, dto.Text, remoteIpAddress);
			if (!result)
			{
				return StatusCode(500);
			}

			return Ok();
		}
	}
}
