﻿using GearShop.Contracts;
using GearShop.Models.ViewModels;
using Humanizer;
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
			var dto = await _gearShopRepository.GetPageContent("ArticlesPage");
			var model = new PageViewModel()
			{
				Id = dto.Id,
				PageContent = dto.Content
			};

			ViewData["Title"] = dto.Title;
			return View(model);
		}

		/// <summary>
		/// Return articles list for create preview page.
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		public async Task<IActionResult> GetArticles(int pageId)
		{
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			var list = await _gearShopRepository.GetArticleList(pageId);
			return Ok(JsonConvert.SerializeObject(list, serializerSettings));
		}

		/// <summary>
		/// Return article content.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
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

			ViewData["Title"] = model.Title;
			return View(model);
		}

		/// <summary>
		/// Return newsfeed.
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> GetNewsfeed(int pageId)
		{
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			var list = await _gearShopRepository.GetNewsfeed(pageId);
			return Ok(JsonConvert.SerializeObject(list, serializerSettings));
		}
	}
}
