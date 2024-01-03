using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using DataParser;
using DataParser.Models;
using DataParser.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteControlApi.Models;

namespace RemoteControlApi
{
	public class RemoteApi
	{
		/// <summary>
		/// Каталог из которого будут загружены файлы.
		/// </summary>
		private const string UploadFolder = "Upload";

		private readonly Action<string> _sendTextToUser;
		private readonly Action<string> _sendErrorToUser;
		private readonly Action<int, int> _printProgress;
		public string LastError { get; private set; }

		private Excel1сShopParser2022Format _parser; 

		public RemoteApi(Action<string> sendTextToUser, Action<string> sendErrorToUser, Action<int, int> printProgress)
		{
			_sendTextToUser = sendTextToUser;
			_sendErrorToUser = sendErrorToUser;
			_printProgress = printProgress;
		}

		public async Task<bool> Authorization()
		{
			WebSender client = new WebSender();
			HttpResponseMessage answer = await client.GetAsync($"{UserData.Host}Login/Authentication", UserData.UserName,
				UserData.Password);

			if (answer.StatusCode != HttpStatusCode.OK)
			{
				LastError = $"Ошибка  {(int)answer.StatusCode}";
				return false;
			}

		//	var headers = answer.R;

			var headers = answer.Headers;
			var headers1 = answer.Headers.ToString();
			IEnumerable<string> values;
			if (headers.TryGetValues("JWToken", out values))
			{
				string session = values.First();
			}

			string responseStream = await answer.Content.ReadAsStringAsync();
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Получает имена картинок и продукты к которым они привязаны.
		/// </summary>
		/// <returns></returns>
		public async Task<List<KeyPair>> GetProductImagesInfo()
		{
			WebSender client = new WebSender();
			HttpResponseMessage answer = await client.GetAsync($"{UserData.Host}UploadData/GetProductImagesInfo", UserData.UserName,
				   UserData.Password);

			if (answer.StatusCode != HttpStatusCode.OK)
			{
				LastError = $"Ошибка  {(int)answer.StatusCode}";
				return null;
			}

			var content = await answer.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<List<KeyPair>>(content);
		}

		public async Task<bool> ParseData()
		{
			_sendTextToUser($"Будет загружен прайс из папки Upload.{Environment.NewLine}");

			string filePath = CreateUploadDir();
			if (filePath == null) return false;
			
			_sendTextToUser($"Получение идентификаторов картинок продуктов с сервера магазина…{Environment.NewLine}");
			var imageInfo = await GetProductImagesInfo();
			if(imageInfo == null) return false;

			_sendTextToUser($"Обработка данных из файла {filePath}.{Environment.NewLine}");
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			_parser = new Excel1сShopParser2022Format();
			_parser.SetImageInfo(imageInfo.Select(m => new KeyValuePair<string, string>(m.Key, m.Value)).ToList());
			
			List<Product> products = ParsePriceFile(filePath);
			if (products == null)
			{
				stopWatch.Stop();
				return false;
			}
			
			_sendTextToUser($"Обработка списка продуктов завершена.");

			string csvFile = Path.Combine(UploadFolder, Path.GetFileName(filePath) + ".txt");
			_sendTextToUser($"{Environment.NewLine}");
			_sendTextToUser($"Сохранение данных в csv формат. Файл {filePath}.{Environment.NewLine}");
			_parser.SaveToСsvFile(products, csvFile, "|");
			_sendTextToUser($"Сохранен.");
			_sendTextToUser($"{Environment.NewLine}");
			_sendTextToUser($"Сохранение картинок...{Environment.NewLine}");

			if (!_parser.SaveImages(_printProgress))
			{
				_sendErrorToUser($"{_parser.LastError}{Environment.NewLine}");
				stopWatch.Stop();
				return false;
			}

			//Убрать после доработок.
			if (!string.IsNullOrEmpty(_parser.LastError))
			{
				_sendErrorToUser($"{_parser.LastError}{Environment.NewLine}");
			}

			stopWatch.Stop();
			TimeSpan interval = stopWatch.Elapsed;
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				interval.Hours, interval.Minutes, interval.Seconds, interval.Milliseconds / 10);

			_sendTextToUser($"{Environment.NewLine}Время обработки {elapsedTime}.{Environment.NewLine}");


			return true;
		}

		/// <summary>
		/// Создает папку окуда будет считат прайс лист. Возвращает первый файл похожий на прайс.
		/// </summary>
		/// <returns></returns>
		private string CreateUploadDir()
		{
			if (!Directory.Exists(UploadFolder))
			{
				Directory.CreateDirectory(UploadFolder);
				_sendTextToUser($"Не найдена папка загрузки{UploadFolder}. Программа создала папку. Поместите в нее файлы и выполните команду повторно.{Environment.NewLine}");
				return null;
			}

			string[] files = Directory.GetFiles(UploadFolder, "*.xlsx", SearchOption.TopDirectoryOnly);
			if (files.Length == 0)
			{
				_sendTextToUser($"Не найден файл прайс листа(*.xlsx) в папке {UploadFolder}. Поместите файл и выполните команду повторно.{Environment.NewLine}");
				return null;
			}

			return files[0];
		}

		private List<Product> ParsePriceFile(string filePath)
		{
			if (!_parser.OpenBook(filePath))
			{
				LastError = $"{_parser.LastError}{Environment.NewLine}";
				return null;
			}

			List<Product> products = _parser.ParseFileFast(_printProgress);
			if (products == null)
			{
				LastError = $"{_parser.LastError}{Environment.NewLine}";
				return null;
			}

			ProductTypesParser productTypesParser = new ProductTypesParser();
			productTypesParser.DefineProductsType(products);
			
			return products;
		}

		/// <summary>
		/// Create backup user files and db. 
		/// </summary>
		/// <returns></returns>
		public async Task<bool> CreateWebSiteBackup(string dstDir)
		{
			CreateDir(dstDir);
			_sendTextToUser($"Create backup wwwroot dir...{Environment.NewLine}");
			bool result = await DownloadRootFiles(UserData.UserName, UserData.Password, dstDir);
			if(!result) return false;

			return true;
		}
		
		/// <summary>
		/// Create backup of user files(images, articles) from wwwroot dir.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		public async Task<bool> DownloadRootFiles(string userName, string password, string dstDir)
		{
			WebSender webSender = new WebSender();
			if (!await webSender.DownloadFile(UserData.BackupRootFiles, UserData.UserName, UserData.Password, dstDir,
				    _printProgress))
			{
				_sendErrorToUser(webSender.LastError);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Create dir if not exists.
		/// </summary>
		/// <param name="dirName"></param>
		private void CreateDir(string dirName)
		{
			if (!Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}
		}
	}
}
