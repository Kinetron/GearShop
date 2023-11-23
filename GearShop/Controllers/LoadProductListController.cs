using GearShop.Contracts;
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
		private readonly IIdentityService _identityService;

		public LoadProductListController(IFileStorage fileStorage, IDataSynchronizer dataSynchronizer, IIdentityService identityService)
		{
			_fileStorage = fileStorage;
			_dataSynchronizer = dataSynchronizer;
			_identityService = identityService;
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
	}
}
