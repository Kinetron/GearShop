﻿namespace GearShop.Contracts;

public interface IDataSynchronizer
{
    /// <summary>
    /// Синхронизирует данные в БД с файлом CSV.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    bool CsvSynchronize(string fileName);
}