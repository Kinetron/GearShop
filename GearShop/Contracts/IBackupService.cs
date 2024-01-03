using Microsoft.AspNetCore.Mvc;

namespace GearShop.Contracts;

/// <summary>
/// Service for create backups.
/// </summary>
public interface IBackupService
{
	public string LastError { get; }

	/// <summary>
	/// Add images to archive and return user.
	/// </summary>
	/// <returns></returns>
	Task<string> ArchivingRootFiles();
}