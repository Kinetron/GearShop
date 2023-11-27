using GearShop.Contracts;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GearShop.Controllers
{
	/// <summary>
	/// Загружает список продуктов в БД.
	/// </summary>
	public class UploadDataController : Controller
	{
		private readonly IFileStorage _fileStorage;
		private readonly IDataSynchronizer _dataSynchronizer;
		private readonly IIdentityService _identityService;
		private readonly IGearShopRepository _gearShopRepository;

		public UploadDataController(IFileStorage fileStorage, IDataSynchronizer dataSynchronizer, 
			IIdentityService identityService, IGearShopRepository gearShopRepository)
		{
			_fileStorage = fileStorage;
			_dataSynchronizer = dataSynchronizer;
			_identityService = identityService;
			_gearShopRepository = gearShopRepository;
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
			if(!_identityService.IsValidUser(userName, password))
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
		}

		/// <summary>
		/// Загружает часть архива в хранилище.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> UploadArchivePart(IFormFile file, string userName, string password)
		{
			if (!_identityService.IsValidUser(userName, password))
			{
				return StatusCode(401);
			}

			if (!await _fileStorage.WriteFile(file))
			{
				return StatusCode(507, _fileStorage.LastError);
			}

			return Ok("dsddss");
		}
		
		/// <summary>
		/// Синхронизация архива с картинками товаров.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> SynchronizeImageArchive(string fileName, string userName, string password)
		{
			if (!_identityService.IsValidUser(userName, password))
			{
				return StatusCode(401);
			}

			if (!_dataSynchronizer.ProductImagesSynchronize(fileName, _fileStorage.StoragePath))
			{
				return StatusCode(507, _dataSynchronizer.LastError);
			}

			return Ok();
		}

		/// <summary>
		/// Возвращает название картинок и название продукта к которому относиться картинка.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<IActionResult> GetProductImagesInfo(string userName, string password)
		{
			if (!_identityService.IsValidUser(userName, password))
			{
				return StatusCode(401);
			}

			var result = await _gearShopRepository.GetProductImagesInfoAsync();
			string json = JsonConvert.SerializeObject(result);

			return Ok(json);
		}
	}
}
