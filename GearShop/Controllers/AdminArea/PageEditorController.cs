using GearShop.Contracts;
using GearShop.Services.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using GearShop.Models.Dto;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddArticle(string data)
		{
			ArticleDto dto = JsonConvert.DeserializeObject<ArticleDto>(data);
			bool result = await _repository.AddArticle(dto);
			if (result)
			{
				return Ok();
			}

			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateArticle(string data)
		{
			ArticleDto dto = JsonConvert.DeserializeObject<ArticleDto>(data);
			bool result = await _repository.UpdateArticle(dto);
			if (result)
			{
				return Ok();
			}

			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteArticle(int id)
		{
			bool result = await _repository.DeleteArticle(id);
			if (result)
			{
				return Ok();
			}

			return BadRequest();
		}
	}
}
