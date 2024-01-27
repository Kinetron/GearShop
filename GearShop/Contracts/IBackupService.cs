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

	/// <summary>
	/// Create and return Db backup file. If it allow in appsetings.json.
	/// </summary>
	/// <returns>Path to file.</returns>
	Task<string> CreateDbBackup();
}