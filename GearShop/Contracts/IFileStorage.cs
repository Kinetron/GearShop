namespace GearShop.Contracts;

/// <summary>
/// Локальное хранилище данных.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    string LastError { get; }

    /// <summary>
    /// Путь к каталогу хранилищя.
    /// </summary>
    string StoragePath { get; }

	/// <summary>
	/// Сохраняет файл.
	/// </summary>
	/// <param name="file"></param>
	/// <returns></returns>
	Task<bool> WriteFile(IFormFile file);

	/// <summary>
	/// Сохраняет файлы статей.
	/// </summary>
	/// <param name="file"></param>
	/// <returns></returns>
	Task<string> SaveArticleFile(IFormFile file);
}