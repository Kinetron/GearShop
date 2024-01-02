using GearShop.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Services
{
    /// <summary>
    /// Service for create backups.
    /// </summary>
    public class BackupService : IBackupService
	{

		/// <summary>
		/// Add images to archive and return user.
		/// </summary>
		/// <returns></returns>
		public IActionResult DownloadImages()
		{
			byte[] fileBytes = System.IO.File.ReadAllBytes("");
			return null;
		}
	}
}
