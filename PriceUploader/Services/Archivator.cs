using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PriceUploader.Services
{
	internal static class Archivator
	{
		public static string ArchiveFolderToZip(string folderPath, string archiveName)
		{
			string zipPath = Path.Combine(Path.GetDirectoryName(folderPath), $"{archiveName}.zip");
			ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Fastest, true);
			return zipPath;
		}

		public static int ArchiveFolderToZip(string folderPath, string archiveName, int maxOutputSegmentSize)
		{
			int segmentsCreated;
			using (var zip = new Ionic.Zip.ZipFile())
			{
				zip.AddDirectory(folderPath);
				zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
				zip.MaxOutputSegmentSize = maxOutputSegmentSize;
				zip.Save(archiveName);

				segmentsCreated = zip.NumberOfSegmentsForMostRecentSave;
			}

			return segmentsCreated;
		}

	}
}
