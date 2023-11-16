using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParser;
using DataParser.Models;
using DataParser.Models.Products;
using DataParser.Services;
using PriceUploader.Contracts;

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

            CsvParser parser = new CsvParser();
            List<Product> products = parser.ParseFile(files[0], '|');
            if (products == null)
            {
                _sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
                return;
            }

            _sendTextToUser($"Файл считан. Разбор данных.");

            ProductTypesParser productTypesParser = new ProductTypesParser();
            productTypesParser.ParseProducts(products);
            var allProducts = productTypesParser.AllProducts;

            EventEndWork?.Invoke();
        }

        public void ReadUserInput(string text)
        {
        }

        public event Action? EventEndWork;
    }
}
