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
using GearShop.Services;

namespace GearShop.Controllers.AdminArea
{
	public class PageEditorController : Controller
	{
		private readonly IGearShopRepository _repository;
		private readonly IFileStorage _fileStorage;

		public PageEditorController(IGearShopRepository repository, IFileStorage fileStorage)
		{
			_repository = repository;
			_fileStorage = fileStorage;
		}
		
		/// <summary>
		/// Обновляет содержимое страницы.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="pageName"></param>
		/// <returns></returns>
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> UpdatePageContent(int id, string text)
		{
			bool result = await _repository.UpdatePageContent(id, text);
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

		/// <summary>
		/// Загружает картинку из статьи.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UploadArticleImage(IFormFile file)
		{
			string url = await _fileStorage.SaveArticleFile(file);
			if (string.IsNullOrEmpty(url))
			{
				return Json(new { error = _fileStorage.LastError });
			}

			return Json(new { url });
		}
	}
}
