using GearShop.Contracts;

namespace GearShop.Services
{
    /// <summary>
    /// Локальное хранилище данных.
    /// </summary>
    public class FileStorage : IFileStorage
	{
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
				var filepath = Path.Combine(Directory.GetCurrentDirectory(), StoragePath);

				if (!Directory.Exists(filepath))
				{
					Directory.CreateDirectory(filepath);
				}

				var exactpath = Path.Combine(Directory.GetCurrentDirectory(), StoragePath, file.FileName);
				using (var stream = new FileStream(exactpath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}
			}
			catch (Exception ex)
			{
				LastError = ex.Message + " " + ex.StackTrace;
				return false;
			}

			return true;
		}
	}
}
