using GearShop.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
	}
}
