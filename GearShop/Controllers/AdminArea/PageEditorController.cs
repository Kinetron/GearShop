using GearShop.Contracts;
using GearShop.Services.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace GearShop.Controllers.AdminArea
{
	public class PageEditorController : Controller
	{
		private readonly IGearShopRepository _repository;

		public PageEditorController(IGearShopRepository repository)
		{
			_repository = repository;
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> Save(string text, string pageName)
		{
			bool result = await _repository.SavePageContent(text, pageName);
			if (result)
			{
				return Ok();
			}

			return BadRequest();
		}

		/// <summary>
		/// Возвращает статью.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<IActionResult> GetArticle(int id)
		{
			var result = await _repository.GetArticle(id);
			if (result != null)
			{
				var serializerSettings = new JsonSerializerSettings();
				serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
				return Ok(JsonConvert.SerializeObject(result, serializerSettings));
			}

			return BadRequest();
		}

		/// <summary>
		/// Добавить статью.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="content"></param>
		/// <param name="parentPageName"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> AddArticle(string title, string content, string parentPageName)
		{
			bool result = await _repository.AddArticle(title, content, parentPageName);
			if (result)
			{
				return Ok();
			}

			return BadRequest();
		}

		[HttpPost]
		public async Task<IActionResult> UpdateArticle(string title, string content, int id)
		{
			bool result = await _repository.UpdateArticle(title, content, id);
			if (result)
			{
				return Ok();
			}

			return BadRequest();
		}
	}
}
