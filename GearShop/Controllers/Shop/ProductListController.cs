﻿using GearShop.Contracts;
using GearShop.Models.Dto.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace GearShop.Controllers.Shop
{
    /// <summary>
    /// Основная страница магазина. Список продуктов.
    /// </summary>
    public class ProductListController : Controller
    {
	    private readonly IGearShopRepository _gearShopRepository;
	    private readonly IDetectionService _detectionService;

	    /// <summary>
		/// Записей на одну отображаемую страницу.
		/// </summary>
		private const int recordPerPage = 9;

	    public ProductListController(IGearShopRepository gearShopRepository, IDetectionService detectionService)
	    {
		    _gearShopRepository = gearShopRepository;
		    _detectionService = detectionService;
	    }

        // GET: ProductListController
        public ActionResult Index()
		{
			ViewData["ProductTypes"] = _gearShopRepository.GetProductTypesAsync().Result
				.Select(x=>new KeyValuePair<int, string>(x.Id, x.Name)).ToList();

			ViewData["IsMobile"] = _detectionService.Device.Type != Device.Desktop;

			return View();
        }

        public JsonResult GetProductList(int currentPage, string searchText, int productTypeId)
        {
	        return Json(_gearShopRepository.GetProducts(currentPage, recordPerPage, searchText, productTypeId));
        }

        /// <summary>
        /// Получает параметры пейдженации страниц.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPaginateData(string searchText, int productTypeId)
        {
	        int totalRecords = _gearShopRepository.GetProductCount(searchText, productTypeId);
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
