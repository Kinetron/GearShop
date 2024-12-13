namespace RemoteControlApi
{
	//Учетка загрузчика.
	public static class UserData
	{
		private static string _hostName = "http://localhost:80/";
		//private static string _hostName = "https://autolugansk.ru/";

		public static string Host = _hostName;
		public static string UserName = "UploaderMan898qw";
		public static string Password = "IpYNrGy5M2TP4eewVdDcII8lOVrHVn2g3c7R5HXHnmPz";
		public static string SynchronizeImageArchive = $"{_hostName}UploadData/SynchronizeImageArchive";
		public static string UploadArchivePart = $"{_hostName}UploadData/UploadArchivePart";

		public static string UploadCsvUrl = $"{_hostName}UploadData/UploadCsv";
		public static string ShopName = "Магазин подшибников";//"Запчасти Matiz";


		public static string GetProductImagesInfo = $"{_hostName}UploadData/GetProductImagesInfo";

		/// <summary>
		/// Возвращает информацию о текущей выполняемой операции(например синхронизация данных).
		/// </summary>
		/// <param name="operationId"></param>
		public static string OperationStatus = $"{_hostName}UploadData/OperationStatus";

		/// <summary>
		/// Url WebApi for backup files from wwwroot dir.
		/// </summary>
		public static string BackupRootFiles = $"{_hostName}Backup/DownloadRootFiles";

		/// <summary>
		/// Url WebApi for download db backup.
		/// </summary>
		public static string DownloadDbBackup = $"{_hostName}Backup/DownloadDbBackup";
	}
}
