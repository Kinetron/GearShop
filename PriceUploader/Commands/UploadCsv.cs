using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DataParser;
using DataParser.Models;
using DataParser.Models.Products;
using DataParser.Services;
using Newtonsoft.Json;
using PriceUploader.Contracts;
using RemoteControlApi;
using RemoteControlApi.Models;
using static System.Net.WebRequestMethods;

namespace PriceUploader.Commands
{
    internal class UploadCsv : ICommand
    {
        /// <summary>
        /// Каталог из которого будут загружены файлы.
        /// </summary>
        private const string UploadFolder = "Upload";

        private readonly Action<string> _sendTextToUser;
        private readonly Action<string> _sendErrorToUser;
        private readonly Action<int, int> _printProgress;
        public string Name { get; } = "csv";
        public string Description { get; } = "Загрузка прайс листа из csv файла на сервер";

		public UploadCsv(Action<string> sendTextToUser, Action<string> sendErrorToUser, Action<int, int> printProgress)
        {
            _sendTextToUser = sendTextToUser;
            _sendErrorToUser = sendErrorToUser;
            _printProgress = printProgress;
        }
        public void Run()
        {
            _sendTextToUser($"Будет загружен прайс из папки Upload с расширением *.txt.{Environment.NewLine}");

            string[] files = Directory.GetFiles(UploadFolder, "*.txt", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                _sendTextToUser($"Не найден файл прайс листа в папке {UploadFolder}. Поместите файл и выполните команду повторно.{Environment.NewLine}");
                EventEndWork?.Invoke();
            }

			_sendTextToUser($"Загрузка файла на сервер...{Environment.NewLine}");
            RemoteControlApi.WebSender fileUploader = new RemoteControlApi.WebSender();

            Task.Run(ShowOperationProgress);

			//Переделать на запуск фонового процесса!
			HttpResponseMessage answer = fileUploader.Upload(UserData.UploadCsvUrl, files[0], UserData.UserName,
				UserData.Password, UserData.ShopName).Result;

			if (answer.StatusCode != HttpStatusCode.OK)
            {
	            _sendTextToUser($"Ошибка загрузки файла: {(int)answer.StatusCode}. Попробуйте еще раз выполнить команду.{Environment.NewLine}");
				EventEndWork?.Invoke();
                return;
			}
        }

		/// <summary>
		/// Получает сведения о прогрессе выполнения операции.
		/// </summary>
		private async Task<bool> ShowOperationProgress()
		{
			await Task.Delay(2000);

			RemoteControlApi.WebSender client = new RemoteControlApi.WebSender();

			while (true)
			{
				await Task.Delay(1000);
				HttpResponseMessage answer = await client.GetByParamAsync(UserData.OperationStatus, "1", "operationId");

				if (answer.StatusCode != HttpStatusCode.OK) continue;
				var content = await answer.Content.ReadAsStringAsync();
				var data = JsonConvert.DeserializeObject<PriceSynchronizeStatus>(content);
				
                _printProgress(data.Current, data.Total);

                if (data.Current == data.Total)
                {
					TimeSpan interval = data.EndOperation.Value - data.BeginOperation.Value;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
	                    interval.Hours, interval.Minutes, interval.Seconds, interval.Milliseconds / 10);

                    _sendTextToUser($"{Environment.NewLine}Время обработки {elapsedTime}.{Environment.NewLine}");
                    break;
				}
			}

			_sendTextToUser($"Успешно.{Environment.NewLine}");
			EventEndWork?.Invoke();
			return true;
		}

        public void ReadUserInput(string text)
        {
        }

        public event Action? EventEndWork;
    }
}
