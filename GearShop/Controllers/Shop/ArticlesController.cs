using GearShop.Contracts;
using GearShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol.Core.Types;

namespace GearShop.Controllers.Shop
{
	/// <summary>
	/// Статьи.
	/// </summary>
	public class ArticlesController : Controller
	{
		private readonly IGearShopRepository _gearShopRepository;

		public ArticlesController(IGearShopRepository gearShopRepository)
		{
			_gearShopRepository = gearShopRepository;
		}
		public async Task<IActionResult> Index()
		{
			ViewData["MainContent"] = await _gearShopRepository.GetChapterContent(null);
			return View();
		}

		public async Task<IActionResult> GetArticles(int pageId)
		{
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			var list = await _gearShopRepository.GetArticleList(pageId);
			return Ok(JsonConvert.SerializeObject(list, serializerSettings));
		}

		public async Task<IActionResult> Article(int id)
		{
			var result = await _gearShopRepository.GetArticle(id);
			var model = new ArticleViewModel()
			{
				Content = "Ошибка"
			};

			if (result != null)
			{
				model.Title = result.Title;
				model.Content = result.Content;
			}

			return View(model);
		}
	}
}
