using GearShop.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
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
		private readonly IServiceScopeFactory _scopeFactory;

		public UploadDataController(IFileStorage fileStorage, IDataSynchronizer dataSynchronizer, 
			IIdentityService identityService, IGearShopRepository gearShopRepository, IServiceScopeFactory scopeFactory)
		{
			_fileStorage = fileStorage;
			_dataSynchronizer = dataSynchronizer;
			_identityService = identityService;
			_gearShopRepository = gearShopRepository;
			_scopeFactory = scopeFactory;
		}

		/// <summary>
		/// Загрузка CSV файла в БД.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> UploadCsv(IFormFile file, string userName, string password, string shopName)
		{
			if(!_identityService.IsValidUser(userName, password))
			{
				return StatusCode(401);
			}
			 
			if (!await _fileStorage.WriteFile(file))
			{
				return StatusCode(507, _fileStorage.LastError);
			}

			//Task.Run(() =>
			//{
			//	using (var scope = _scopeFactory.CreateScope())
			//	{
			//		var synchronizer = scope.ServiceProvider.GetRequiredService<IDataSynchronizer>();

			//	}
			//});

			//Переделать на нормальный вид! с фоновым процессом.
			await _dataSynchronizer.CsvSynchronize(Path.Combine(_fileStorage.StoragePath, file.FileName), shopName);

			return Ok();
		}

		/// <summary>
		/// Возвращает информацию о текущей выполняемой операции(например синхронизация данных)
		/// </summary>
		/// <param name="operationId"></param>
		/// <returns></returns>
		public async Task<IActionResult> OperationStatus(int operationId)
		{
			string result = await _dataSynchronizer.GetOperationStatus(operationId);
			if (result == null)
			{
				return StatusCode(507);
			}
			return Ok(result);
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
