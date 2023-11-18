using GearShop.Contracts;
using GearShop.Models.Dto.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.Shop
{
    public class ProductListController : Controller
    {
	    private readonly IGearShopRepository _gearShopRepository;

		/// <summary>
		/// Записей на одну отображаемую страницу.
		/// </summary>
		private const int recordPerPage = 9;

	    public ProductListController(IGearShopRepository gearShopRepository)
	    {
		    _gearShopRepository = gearShopRepository;
	    }

        // GET: ProductListController
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetProductList(int currentPage, string searchText)
        {
	        return Json(_gearShopRepository.GetProducts(currentPage, recordPerPage, searchText));
        }

        /// <summary>
        /// Получает параметры пейдженации страниц.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPaginateData(string searchText)
        {
	        int totalRecords = _gearShopRepository.GetProductCount(searchText);
            int rows = totalRecords / recordPerPage;

	        return Json(new {rows = rows, totalRecords = totalRecords});
        }

		// GET: ProductListController/Details/5
		public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductListController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductListController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductListController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductListController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductListController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductListController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
