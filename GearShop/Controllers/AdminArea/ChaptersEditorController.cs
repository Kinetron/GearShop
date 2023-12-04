using GearShop.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.AdminArea
{
	/// <summary>
	/// Редактирование статей на сайте
	/// </summary>
	public class ChaptersEditorController : Controller
	{
		private readonly IGearShopRepository _gearShopRepository;

		public ChaptersEditorController(IGearShopRepository gearShopRepository)
		{
			_gearShopRepository = gearShopRepository;
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Index()
		{
			ViewData["MainContent"] = await _gearShopRepository.GetChapterContent(null);
			return View();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> Save(string text, int? chapterId)
		{
			bool result = await _gearShopRepository.SaveChapter(text, chapterId);
			if (result)
			{
				return Ok();
			}
			
			return BadRequest();
		}
	}
}
