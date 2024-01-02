using GearShop.Contracts;
using GearShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.AdminArea
{
	public class BackupController : Controller
	{
		private readonly IIdentityService _identityService;

		public BackupController(IIdentityService identityService)
		{
			_identityService = identityService;
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

			try
			{
				var fs = new FileStream("Program.zip", FileMode.Open);
				return File(fs, "application/octet-stream", "Program.zip");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
	}
}
