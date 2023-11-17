﻿using GearShop.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers
{
	/// <summary>
	/// Загружает список продуктов в БД.
	/// </summary>
	public class LoadProductListController : Controller
	{
		private readonly IFileStorage _fileStorage;
		private readonly IDataSynchronizer _dataSynchronizer;

		public LoadProductListController(IFileStorage fileStorage, IDataSynchronizer dataSynchronizer)
		{
			_fileStorage = fileStorage;
			_dataSynchronizer = dataSynchronizer;
		}

		/// <summary>
		/// Загрузка CSV файла в БД.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> UploadCsv(IFormFile file, string userName, string password)
		{
			//Добавить авторизацию через бд.
			if (userName != "UploaderMan898qw" || password != "IpYNrGy5M2TP4eewVdDcII8lOVrHVn2g3c7R5HXHnmPz")
			{
				return StatusCode(401);
			}
			 
			if (!await _fileStorage.WriteFile(file))
			{
				return StatusCode(507, _fileStorage.LastError);
			}

			if (!_dataSynchronizer.CsvSynchronize(Path.Combine(_fileStorage.StoragePath, file.FileName)))
			{
				return StatusCode(507);
			}
				return Ok("dsddss");
			//return BadRequest("zzzzzz45");
		}
	}
}