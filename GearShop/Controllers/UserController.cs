using GearShop.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GearShop.Controllers
{
	public class UserController : Controller
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IGearShopRepository _gearShopRepository;

		public UserController(IHttpContextAccessor httpContextAccessor, IGearShopRepository gearShopRepository)
		{
			_httpContextAccessor = httpContextAccessor;
			_gearShopRepository = gearShopRepository;
		}

		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Возвращает идентификатор для не зарегистрированного пользователя.
		/// Система создает нового не зарегистрированного пользователя.
		/// А на стороне пользователя guid сохраняется в localStorage.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public JsonResult GetGuidFoNoRegistered()
		{
			var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
			return Json(new { guid = Guid.NewGuid() });
		}

		/// <summary>
		/// Проверяет наличии данного идентификатора в БД. Если нет – добавляет пользователя. 
		/// Возвращает идентификатор. 
		/// </summary>
		/// <returns></returns>
		public JsonResult GeNoRegisteredOrAdd()
		{
			return Json(new { guid = Guid.NewGuid() });
		}

		/// <summary>
		/// Синхронизирует уникальный идентификатор не зарегистрированного пользователя на стороне браузера и guid на стороне сервера.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public async Task<IActionResult> SynchronizeNoRegUserGuid(string guid)
		{
			var ip = GetRemoteIp();
			bool result = await _gearShopRepository.SynchronizeNoRegUserGuidAsync(guid, ip);
			if (result)
			{
				return Ok();
			}

			return StatusCode(500);
		}

		private string GetRemoteIp()
		{
			string remoteIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
			if (Request.Headers.ContainsKey("X-Forwarded-For"))
				remoteIpAddress = Request.Headers["X-Forwarded-For"];

			return remoteIpAddress;
		}
	}
}
