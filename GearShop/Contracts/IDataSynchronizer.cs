namespace GearShop.Contracts;

public interface IDataSynchronizer
{
    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
	string LastError { get; }

	/// <summary>
	/// Синхронизирует данные в БД с файлом CSV.
	/// </summary>
	/// <param name="fileName"></param>
	/// <returns></returns>
	Task<bool> CsvSynchronize(string fileName, string shopName);

    /// <summary>
    /// Синхронизация картинок продуктов.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    bool ProductImagesSynchronize(string fileName, string storagePath);

	/// <summary>
	/// Возвращает информацию о текущей выполняемой операции(например синхронизация данных).
	/// </summary>
	/// <param name="operationId"></param>
	/// <returns></returns>
	Task<string> GetOperationStatus(int operationId);
}