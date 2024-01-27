using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GearShop.Services
{
	internal static class Archivator
	{
		public static string LastError;
		public static string? ArchiveFolderToZip(string folderPath, string archiveName)
		{
			try
			{
				string? zipPath = Path.Combine(Path.GetDirectoryName(folderPath), $"{archiveName}.zip");
				ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Fastest, true);
				return zipPath;
			}
			catch (Exception ex)
			{
				LastError = $"{ex.Message} {ex.StackTrace}";
				return null;
			}
		}

		public static void UnpackZipToFolder(string zipPath, string extractPath)
		{
			ZipFile.ExtractToDirectory(zipPath, extractPath);
		}

		/// <summary>
		/// Архив разбитый на части.
		/// </summary>
		/// <param name="zipPath"></param>
		/// <param name="extractPath"></param>
		public static int UnpackSplitZip(string zipPath, string extractPath)
		{
			int partCount = 0;
			using (var zip = Ionic.Zip.ZipFile.Read(zipPath))
			{
				foreach (var item in zip)
				{
					item.Extract(extractPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
					partCount++;
				}
			}

			return partCount;
		}

		public static string? ArchiveFileToZip(string path, string directoryPathInArchive)
		{
			try
			{
				string? zipPath = Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}.zip");
				using (var zip = new Ionic.Zip.ZipFile())
				{
					zip.AddFile(path, directoryPathInArchive);
					zip.Save(zipPath);
				}

				return zipPath;
			}
			catch (Exception ex)
			{
				LastError = $"{ex.Message} {ex.StackTrace}";
				return null;
			}
		}
	}
}
