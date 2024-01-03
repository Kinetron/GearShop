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
		/// Dir name where will save wwwroot backups.
		/// </summary>
	    private const string WebFilesBackupsDir = "WebBackups";

		public string LastError { get; private set; }

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
	}
}
