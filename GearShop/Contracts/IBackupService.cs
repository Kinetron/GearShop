using Microsoft.AspNetCore.Mvc;

namespace GearShop.Contracts;

/// <summary>
/// Service for create backups.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Add images to archive and return user.
    /// </summary>
    /// <returns></returns>
    IActionResult DownloadImages();
}