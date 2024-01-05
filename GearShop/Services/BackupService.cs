using GearShop.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Serilog;

namespace GearShop.Services
{
    /// <summary>
    /// Service for create backups.
    /// </summary>
    public class BackupService : IBackupService
    {
	    private readonly bool _allowDownloadDbBackup;
	    private readonly string _pathToDbBackupFiles;
	    private readonly IGearShopRepository _gearShopRepository;

	    /// <summary>
		/// Dir name where will save wwwroot backups.
		/// </summary>
	    private const string WebFilesBackupsDir = "WebBackups";

		/// <summary>
		/// Sleep interval when we wait db backup file from sustem job. 
		/// </summary>
	    private const int CheckExistDbFileDelay = 5;

		/// <summary>
		/// Max wait db file.
		/// </summary>
	    private const int MaxCheckExistDbFileDelay = 60;

		public string LastError { get; private set; }

		public BackupService(bool allowDownloadDbBackup, string pathToDbBackupFiles, IGearShopRepository gearShopRepository)
		{
			_allowDownloadDbBackup = allowDownloadDbBackup;
			_pathToDbBackupFiles = pathToDbBackupFiles;
			_gearShopRepository = gearShopRepository;

			if (gearShopRepository == null)
			{
				throw new ArgumentNullException(nameof(gearShopRepository));
			}

			if (string.IsNullOrEmpty(pathToDbBackupFiles))
			{
				throw new ArgumentNullException(nameof(pathToDbBackupFiles));
			}
		}
		
		/// <summary>
		/// Add products images and other files to archive and return path to archive.
		/// </summary>
		/// <returns></returns>
		public async Task<string> ArchivingRootFiles()
		{
			try
			{
				CreateDir(WebFilesBackupsDir);

				string dirName = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");
				string backupDirPath = Path.Combine(WebFilesBackupsDir, dirName);
				CreateDir(backupDirPath);
				CopyDirectory("wwwroot", backupDirPath);

				//Change to async method.
				return Archivator.ArchiveFolderToZip(backupDirPath, $"backup_{dirName}");
			}
			catch (Exception e)
			{
				LastError = $"{e.Message} {e.StackTrace}";
				return null;
			}
		}

		/// <summary>
		/// Create and return Db backup file. If it allow in appsetings.json.
		/// </summary>
		/// <returns>Path to file.</returns>
		public async Task<string> CreateDbBackup()
		{
			if (!_allowDownloadDbBackup)
			{
				LastError = "Deny create db backup. Check appsettings.json";
				return null;
			}

			//Run backup procedure, which create file in directory not allow from web service.
			string path = await _gearShopRepository.BackupDbAsync();
			if (path == null)
			{
				LastError = "Can't create db backup.";
				return null;
			}

			path = Path.Combine(_pathToDbBackupFiles, Path.GetFileName(path));

			//Wait while linux job copy db file to web app dir.
			FileInfo fi = new FileInfo(path);
			int maxRetry = MaxCheckExistDbFileDelay / CheckExistDbFileDelay;
			int loopCount = 0;
			while (!fi.Exists)
			{
				if (loopCount > maxRetry)
				{
					LastError = "System job don't copy backup file.";
					return null;
				}

				await Task.Delay(CheckExistDbFileDelay * 1000);
				loopCount+= CheckExistDbFileDelay;

				fi = new FileInfo(path);
			}

			path = Archivator.ArchiveFileToZip(path, string.Empty);
			if (path == null)
			{
				LastError = Archivator.LastError;
				return null;
			}

			return path;
		}

		/// <summary>
		/// Create dir if not exists.
		/// </summary>
		/// <param name="dirName"></param>
		private void CreateDir(string dirName)
		{
			if (!Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}
		}

		/// <summary>
		/// Удаляет все файлы в директории.
		/// </summary>
		/// <param name="dir"></param>
		private void CleanDir(string dir)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				FileInfo fi = new FileInfo(file);
				fi.Delete();
			}
		}

		/// <summary>
		/// Copy files from src dir to dst.
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="dstDir"></param>
		private void CopyFiles(string srcDir, string dstDir)
		{
			List<string> files = Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories).ToList();

			foreach (string file in files)
			{
				FileInfo mFile = new FileInfo(file);
				mFile.CopyTo(Path.Combine(dstDir, Path.GetFileName(file)), true);
			}
		}

		/// <summary>
		/// Deep copy include subdirectories.
		/// </summary>
		/// <param name="sourceDir"></param>
		/// <param name="destDir"></param>
		/// <returns></returns>
		private void CopyDirectory(string srcDir, string dstDir)
		{
			CreateDir(dstDir);

			DirectoryInfo dir = new DirectoryInfo(srcDir);

			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(dstDir, file.Name);
				file.CopyTo(targetFilePath);
			}

			// Cache directories before we start copying
			DirectoryInfo[] dirs = dir.GetDirectories();

			foreach (DirectoryInfo subDir in dirs)
			{
				string destinationDir = Path.Combine(dstDir, subDir.Name);
				CopyDirectory(subDir.FullName, destinationDir);
			}
		}

		/// <summary>
		/// Returns recently written File from the specified directory.
		/// If the directory does not exist or doesn't contain any file, null is returned.
		/// </summary>
		/// <param name="directoryPath">Path of the directory that needs to be scanned</param>
		/// <returns></returns>
		public string NewestFileOfDirectory(string directoryPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
			if (directoryInfo == null || !directoryInfo.Exists)
			{
				return null;
			}

			FileInfo[] files = directoryInfo.GetFiles();
			DateTime recentWrite = DateTime.MinValue;
			FileInfo recentFile = null;

			foreach (FileInfo file in files)
			{
				if (file.LastWriteTime > recentWrite)
				{
					recentWrite = file.LastWriteTime;
					recentFile = file;
				}
			}

			return recentFile.Name;
		}
	}
}
