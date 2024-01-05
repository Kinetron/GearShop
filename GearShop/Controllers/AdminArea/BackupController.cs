using GearShop.Contracts;
using GearShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.AdminArea
{
	public class BackupController : Controller
	{
		private readonly IIdentityService _identityService;
		private readonly IBackupService _backupService;

		public BackupController(IIdentityService identityService, IBackupService backupService)
		{
			_identityService = identityService;
			_backupService = backupService;
		}
		
		/// <summary>
		/// Create backup of user files(images, articles) from wwwroot dir.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		[HttpPost]
		public async Task<IActionResult> DownloadRootFiles(string userName, string password)
		{
			if (!_identityService.IsValidUser(userName, password))
			{
				return StatusCode(401);
			}

			string path = await _backupService.ArchivingRootFiles();
			if (path == null)
			{
				return StatusCode(500, _backupService.LastError);
			}

			FileStream fs = new FileStream(path, FileMode.Open);
			return File(fs, "application/octet-stream", Path.GetFileName(path));
		}

		/// <summary>
		/// Create and return Db backup file. If it allow in appsettings.json.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> DownloadDbBackup(string userName, string password)
		{
			if (!_identityService.IsValidUser(userName, password))
			{
				return StatusCode(401);
			}

			string path = await _backupService.CreateDbBackup();
			if (path == null)
			{
				return StatusCode(500, _backupService.LastError);
			}

			FileStream fs = new FileStream(path, FileMode.Open);
			return File(fs, "application/octet-stream", Path.GetFileName(path));
		}
	}
}
