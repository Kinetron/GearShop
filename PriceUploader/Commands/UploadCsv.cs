using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DataParser;
using DataParser.Models;
using DataParser.Models.Products;
using DataParser.Services;
using PriceUploader.Contracts;
using RemoteControlApi;
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

			//CsvParser parser = new CsvParser();
			//List<Product> products = parser.ParseFile(files[0], '|');
			//if (products == null)
			//{
			//    _sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
			//    return;
			//}

			//Разбивка по типам товара. Не реализована.
			//_sendTextToUser($"Файл считан. Разбор данных.");
			//ProductTypesParser productTypesParser = new ProductTypesParser();
			//productTypesParser.ParseProducts(products);
			//var allProducts = productTypesParser.AllProducts;

			_sendTextToUser($"Загрузка файла на сервер...{Environment.NewLine}");
            RemoteControlApi.WebClient fileUploader = new RemoteControlApi.WebClient();

            //string url = "http://autolugansk.ru/LoadProductList/UploadCsv";
			//string url = "https://localhost:44342/LoadProductList/UploadCsv";
			//HttpResponseMessage answer = fileUploader.Upload(url, 
			//  files[0], "UploaderMan898qw", "IpYNrGy5M2TP4eewVdDcII8lOVrHVn2g3c7R5HXHnmPz").Result;

			HttpResponseMessage answer = fileUploader.Upload(UserData.UploadCsvUrl, files[0], UserData.UserName,
				UserData.Password).Result;

			if (answer.StatusCode != HttpStatusCode.OK)
            {
	            _sendTextToUser($"Ошибка загрузки файла: {(int)answer.StatusCode}. Попробуйте еще раз выполнить команду.{Environment.NewLine}");
				EventEndWork?.Invoke();
                return;
			}

            _sendTextToUser($"Успешно.{Environment.NewLine}");
            EventEndWork?.Invoke();
        }

        public void ReadUserInput(string text)
        {
        }

        public event Action? EventEndWork;
    }
}
