using GearShop.Contracts;
using System.IO;

namespace GearShop.Services
{
    /// <summary>
    /// Локальное хранилище данных.
    /// </summary>
    public class FileStorage : IFileStorage
    {
		/// <summary>
		/// Каталог где храниться картинки и прикрепленные файлы статей. 
		/// </summary>
		private const string ArticleFilesDir = "article-files";

		/// <summary>
		/// Максимальный размер файла в статьях(картинки).
		/// </summary>
		private const int MaxArticleFileSize = 2 * 1024 * 1024;

		/// <summary>
		/// Сообщение об ошибке.
		/// </summary>
		public string LastError { get; private set; }

		/// <summary>
		/// Путь к каталогу хранилищя.
		/// </summary>
		public string StoragePath { get; }
		
		public FileStorage(string storagePath)
		{
			StoragePath = storagePath;

			//Создаем каталог хранилища, если не существует.
			var filepath = Path.Combine(Directory.GetCurrentDirectory(), StoragePath);

			if (!Directory.Exists(filepath))
			{
				Directory.CreateDirectory(filepath);
			}

			string articlesDir = Path.Combine(filepath, ArticleFilesDir);
			//Каталог хранения данных статей.
			if (!Directory.Exists(articlesDir))
			{
				Directory.CreateDirectory(articlesDir);
			}
		}
		
		/// <summary>
		/// Сохраняет файл.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public async Task<bool> WriteFile(IFormFile file)
		{
			try
			{
				string path = Path.Combine(Directory.GetCurrentDirectory(), StoragePath, file.FileName);
				await SaveFile(file, path);
			}
			catch (Exception ex)
			{
				LastError = ex.Message + " " + ex.StackTrace;
				return false;
			}

			return true;
		}

		private async Task SaveFile(IFormFile file, string path)
		{
			using (var stream = new FileStream(path, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}
		}

		/// <summary>
		/// Сохраняет файлы статей.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public async Task<string> SaveArticleFile(IFormFile file)
		{
			if (file.Length > MaxArticleFileSize)
			{
				LastError = $"Размер файла не может быть больше {MaxArticleFileSize/(1024 * 1024)} Мб";
				return null;
			}

			//Файлы группируются по дням.
			string path = Path.Combine("wwwroot", ArticleFilesDir, DateTime.Now.ToString("dd-MM-yyyy"));

			try
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				string filepath = Path.Combine(path, file.FileName);
				await SaveFile(file, filepath);
				
				return filepath.Replace("wwwroot", "");
			}
			catch (Exception ex)
			{
				LastError = ex.Message + " " + ex.StackTrace;
				return null;
			}
		}
	}
}
