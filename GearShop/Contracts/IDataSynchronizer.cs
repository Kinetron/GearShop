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
    bool CsvSynchronize(string fileName);

    /// <summary>
    /// Синхронизация картинок продуктов.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    bool ProductImagesSynchronize(string fileName, string storagePath);
}