namespace PriceUploader
{
	//Учетка загрузчика.
	internal static class UserData
	{
		//private static string _hostName = "https://localhost:44342/";
		private static string _hostName = "http://autolugansk.ru/";

		public static string UserName = "UploaderMan898qw";
		public static string Password = "IpYNrGy5M2TP4eewVdDcII8lOVrHVn2g3c7R5HXHnmPz";
		public static string SynchronizeImageArchive = $"{_hostName}UploadData/SynchronizeImageArchive";
		public static string UploadArchivePart = $"{_hostName}UploadData/UploadArchivePart";

		public static string UploadCsvUrl = $"{_hostName}UploadData/UploadCsv";
		public static string GetProductImagesInfo = $"{_hostName}UploadData/GetProductImagesInfo";
	}
}
