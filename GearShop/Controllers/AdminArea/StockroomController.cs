using GearShop.Contracts;
using GearShop.Models.Dto.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GearShop.Controllers.AdminArea
{
	/// <summary>
	/// Изменения на складе.
	/// </summary>
	public class StockroomController : Controller
	{
		private readonly IGearShopRepository _gearShopRepository;

		/// <summary>
		/// Записей на одну отображаемую страницу.
		/// </summary>
		private const int recordPerPage = 9;

		public StockroomController(IGearShopRepository gearShopRepository)
		{
			_gearShopRepository = gearShopRepository;
		}

		[Authorize(Roles = "Admin")]
		public async Task<JsonResult> GetProductsFromStockroom(int currentPage, string searchText, int productTypeId, bool available)
		{
			return Json(await _gearShopRepository.GetProductsFromStockroom(currentPage, recordPerPage, searchText, productTypeId, available));
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> CreateProduct(string json)
		{
			ProductDto model = JsonConvert.DeserializeObject<ProductDto>(json);
			bool result = await _gearShopRepository.CreateProductAsync(model);

			if (result) return Ok();

			return BadRequest();
		}


		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> UpdateProduct(string json)
		{
			ProductDto model = JsonConvert.DeserializeObject<ProductDto>(json);
			bool result = await _gearShopRepository.UpdateProductAsync(model);

			if (result) return Ok();

			return BadRequest();
		}


		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			bool result = await _gearShopRepository.DeleteProductAsync(id);

			if (result) return Ok();

			return BadRequest();
		}
	}
}
