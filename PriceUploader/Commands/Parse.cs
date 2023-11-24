using DataParser.Models;
using DataParser;
using PriceUploader.Contracts;
using System.IO;
using System.Diagnostics;
using DataParser.Services;

namespace PriceUploader.Commands
{
    public class Parse : ICommand
    {
        /// <summary>
        /// Каталог из которого будут загружены файлы.
        /// </summary>
        private const string UploadFolder = "Upload";

        private readonly Action<string> _sendTextToUser;
        private readonly Action<string> _sendErrorToUser;
        private readonly Action<int, int> _printProgress;
        public string Name { get; } = "ps";
        public string Description { get; } = "Загрузка прайс листа на сервер.";

        public Parse(Action<string> sendTextToUser, Action<string> sendErrorToUser, Action<int, int> printProgress)
        {
            _sendTextToUser = sendTextToUser;
            _sendErrorToUser = sendErrorToUser;
            _printProgress = printProgress;
        }
        public void Run()
        {
            _sendTextToUser($"Будет загружен прайс из папки Upload.{Environment.NewLine}");

            if (!Directory.Exists(UploadFolder))
            {
                Directory.CreateDirectory(UploadFolder);
                _sendTextToUser($"Не найдена папка загрузки{UploadFolder}. Программа создала папку. Поместите в нее файлы и выполните команду повторно.{Environment.NewLine}");
                EventEndWork?.Invoke();
            }

            string[] files = Directory.GetFiles(UploadFolder, "*.xlsx", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                _sendTextToUser($"Не найден файл прайс листа в папке {UploadFolder}. Поместите файл и выполните команду повторно.{Environment.NewLine}");
                EventEndWork?.Invoke();
            }

            Task.Run(() => UploadData(files[0]));
        }

        /// <summary>
        /// Загрузка данных.
        /// </summary>
        /// <param name="filePath"></param>
        private void UploadData(string filePath)
        {
            _sendTextToUser($"Обработка данных из файла {filePath}.{Environment.NewLine}");

            Excel1сShopParser2022Format parser = new Excel1сShopParser2022Format();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            if (!parser.OpenBook(filePath))
            {
	            _sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
	            stopWatch.Stop();
	            return;
			}

            List<Product> products = parser.ParseFileFast(_printProgress);
            if (products == null)
            {
                _sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
                stopWatch.Stop();
                return;
            }

            ProductTypesParser productTypesParser = new ProductTypesParser();
            productTypesParser.DefineProductsType(products);


			_sendTextToUser($"Обработка списка продуктов завершена.");

			string csvFile = Path.Combine(UploadFolder, Path.GetFileName(filePath) + ".txt");
            _sendTextToUser($"{Environment.NewLine}");
            _sendTextToUser($"Сохранение данных в csv формат. Файл {filePath}.{Environment.NewLine}");
            parser.SaveToСsvFile(products, csvFile, "|");
            _sendTextToUser($"Сохранен.");
            _sendTextToUser($"{Environment.NewLine}");
            _sendTextToUser($"Сохранение картинок...{Environment.NewLine}");

			if (!parser.SaveImages(_printProgress))
            {
	            _sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
	            stopWatch.Stop();
	            return;
			}

            //Убрать после доработок.
            if (!string.IsNullOrEmpty(parser.LastError))
            {
	            _sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
			}

            stopWatch.Stop();
            TimeSpan interval = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                interval.Hours, interval.Minutes, interval.Seconds, interval.Milliseconds / 10);

            _sendTextToUser($"{Environment.NewLine}Время обработки {elapsedTime}.{Environment.NewLine}");

            EventEndWork?.Invoke();
        }

        public void ReadUserInput(string text)
        {
            
        }

        public event Action? EventEndWork;
    }
}
