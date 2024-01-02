using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PriceUploader.Contracts;
using RemoteControlApi;
using WebClient = RemoteControlApi.WebSender;

namespace PriceUploader.Commands
{
	public class UploadImages : ICommand
	{
		private readonly Action<string> _sendTextToUser;
		private readonly Action<string> _sendErrorToUser;
		private readonly Action<int, int> _printProgress;
		public string Name { get; } = "ig";
		public string Description { get; } = "Загрузка картинок на сервер";
		
		public UploadImages(Action<string> sendTextToUser, Action<string> sendErrorToUser, Action<int, int> printProgress)
		{
			_sendTextToUser = sendTextToUser;
			_sendErrorToUser = sendErrorToUser;
			_printProgress = printProgress;
		}

		public void Run()
		{
			if (!Directory.Exists(FoldersName.ImageFolder))
			{
				_sendTextToUser($"Не найден каталог{FoldersName.ImageFolder}.{Environment.NewLine}");
				EventEndWork?.Invoke();
			}

			string archiveDir = "Archive";
			if (!Directory.Exists(archiveDir))
			{
				Directory.CreateDirectory(archiveDir);
			}

			_sendTextToUser($"Архивирование картинок...{Environment.NewLine}");

			string archiveName = "image.zip";
			string archiveFile = Path.Combine(archiveDir,"image.zip");
			
			//Разбивка архива на 1Мб
			Archivator.ArchiveFolderToZip(FoldersName.ImageFolder, archiveFile, 1024 * 1024);
			
			var listArhives = Directory.GetFiles(archiveDir, "*.*", SearchOption.AllDirectories);


			_sendTextToUser($"Загрузка файлов на сервер...{Environment.NewLine}");
            RemoteControlApi.WebSender fileUploader = new RemoteControlApi.WebSender();

			int total = listArhives.Length;
			int current = 1;
			foreach (var file in listArhives)
			{
				HttpResponseMessage answer = fileUploader.Upload(UserData.UploadArchivePart, file, UserData.UserName,
					UserData.Password, UserData.ShopName).Result;

				if (answer.StatusCode != HttpStatusCode.OK)
				{
					_sendTextToUser($"Ошибка загрузки файла: {(int)answer.StatusCode}. Попробуйте еще раз выполнить команду.{Environment.NewLine}");
					EventEndWork?.Invoke();
					return;
				}

				_sendTextToUser($"Загружен {current} из {total} {Environment.NewLine}");
				current++;
			}

			CleanDir(archiveDir);

			_sendTextToUser($"Cинхронизация...{Environment.NewLine}");
			KeyValuePair<string, string> content = new KeyValuePair<string, string>("fileName", archiveName);

			HttpResponseMessage result = fileUploader.PostAsync(UserData.SynchronizeImageArchive,content, UserData.UserName,
					UserData.Password).Result;

			if (result.StatusCode != HttpStatusCode.OK)
			{
				string error = result.Content.ReadAsStringAsync().Result;

				_sendTextToUser($"Ошибка загрузки файла: {(int)result.StatusCode} {error}. Попробуйте еще раз выполнить команду.{Environment.NewLine}");
				EventEndWork?.Invoke();
				return;
			}

			_sendTextToUser($"Успешно.{Environment.NewLine}");
			EventEndWork?.Invoke();
		}

		/// <summary>
		/// Удаляет все файлы в директории.
		/// </summary>
		/// <param name="dir"></param>
		private void CleanDir(string dir)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				FileInfo fi = new FileInfo(file);
				fi.Delete();
			}
		}

		public void ReadUserInput(string text)
		{
			
		}

		public event Action? EventEndWork;
	}
}
