using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataParser.Helpers;
using DataParser.Models;
using DataParser.Services;
using IronXL;
using TradesoftPriceConverter.Models;

namespace TradesoftPriceConverter
{
	public class PriceConverter
	{
		private readonly Action<string> _sendTextToUser;
		private readonly Action<string> _sendErrorToUser;

		private WorkBook _workBook = null;

		/// <summary>
		/// Сообщение об ошибке.
		/// </summary>
		public string LastError { get; private set; }

		/// <summary>
		/// Путь к обрабатываемому файлу.
		/// </summary>
		private string _workBookPath = null;

		/// <summary>
		/// Имена свойств заполняемой модели из строки Excel файла.
		/// Последовательность названий должна соответствовать последовательности ячеек в файле.
		/// </summary>
		private readonly string[] _productModelPropertyNames =
		{
			"Brand", "Name", "Article", "Vnutr", "Nar", "Shirina", "ImportBearingName"
		};

		//Сolumn names in the resulting file.
		private readonly string[] _resultFileColumns =
		{
			"Brand", "Name", "Article", "Vnutr", "Nar", "Shirina", "ImportBearingName"
		};

		private readonly string[] _lettersArray =
		{
			"A", "B", "C", "D", "E", "F", "G", "H", "I",
			"J", "K", "L", "M", "N", "O", "P", "Q", "R",
			"S", "T", "U", "V", "W", "X", "Y", "Z", "AA",
			"AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI",
			"AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ",
			"AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ"
		};

		const string ResultXlsFileName = "result.xls";
		const string ResultCsvFileName = "result.csv";

		public PriceConverter(Action<string> sendTextToUser, Action<string> sendErrorToUser)
		{
			_sendTextToUser = sendTextToUser;
			_sendErrorToUser = sendErrorToUser;
		}

		public bool ConvertFile(string folderPath)
		{
			string filePath = CreateFilesDir(folderPath);
			if (filePath == null) return false;

			_workBookPath = filePath;

			if (!OpenBook(filePath))
			{
				LastError = $"{LastError}{Environment.NewLine}";
				return false;
			}

			List<TradesoftProduct> products = ParseFile();
			if (products == null)
			{
				LastError = $"{LastError}{Environment.NewLine}";
				return false;
			}

			FilterProducts(products);
			GetBrand(products);//Gets the manufacturer of the product.
			GetBearingSizes(products); //Fills dimensions.

			GetImportBearingName( products);
			SaveResultFile(products, Path.Combine(folderPath, ResultXlsFileName));
			SaveToCsv(products, Path.Combine(folderPath, ResultCsvFileName));

			return true;
		}


		/// <summary>
		/// Создает папку окуда будет считат прайс лист. Возвращает первый файл похожий на прайс.
		/// </summary>
		/// <returns></returns>
		private string CreateFilesDir(string folderName)
		{
			if (!Directory.Exists(folderName))
			{
				Directory.CreateDirectory(folderName);
				_sendTextToUser($"Не найдена папка загрузки{folderName}. Программа создала папку. Поместите в нее файлы и выполните команду повторно.{Environment.NewLine}");
				return null;
			}

			string[] files = Directory.GetFiles(folderName, "*.xlsx", SearchOption.TopDirectoryOnly);
			if (files.Length == 0)
			{
				_sendTextToUser($"Не найден файл прайс листа(*.xlsx) в папке {folderName}. Поместите файл и выполните команду повторно.{Environment.NewLine}");
				return null;
			}

			return files[0];
		}

		/// <summary>
		/// Открывает эксель файл
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public bool OpenBook(string filePath)
		{
			// Считывание данных с файла по пути filePath
			try
			{
				_workBook = WorkBook.Load(filePath);
				_workBookPath = filePath;
			}
			catch (IOException ex)
			{
				LastError = $"Ошибка при чтении файла {Path.GetFileName(filePath)}.";
				return false;
			}
			catch (Exception ex)
			{
				LastError = $"Исключение: {ex.Message}";
				return false;
			}

			return true;
		}

		/// <summary>
		/// Ускоренный алгоритм.
		/// </summary>

		public List<TradesoftProduct> ParseFile()
		{
			WorkSheet sheet = _workBook.WorkSheets[0]; // Выбор первого листа
			string tempFile = Path.Combine(Path.GetDirectoryName(_workBookPath), "temp.txt");

			//Сохранение в csv excel из 12000 строк занимает 5сек, и его обработка гораздо быстрее чем проход по таблице.
			sheet.SaveAsCsv(tempFile, "|");
			var products = ParseRawCsv(tempFile, '|');
			File.Delete(tempFile);
			return products;
		}

		/// <summary>
		/// Обрабатывает не фильтрованный файл. 12 000 строк обрабатываются почти мгновенно.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private List<TradesoftProduct> ParseRawCsv(string filePath, char separator)
		{
			List<TradesoftProduct> products = new List<TradesoftProduct>();

			int linePos = 0; //skipping the first line.
			using (StreamReader file = new StreamReader(filePath))
			{
				string line = null;

				try
				{
					while ((line = file.ReadLine()) != null)
					{
						if (linePos == 0)
						{
							linePos = 1;
							continue;
						}

						if (string.IsNullOrEmpty(line)) break; //Конец файла.

						string[] columns = line.Split(separator);

						//skipping an empty product name
						if (string.IsNullOrEmpty(columns[1])) continue;

						TradesoftProduct product = ParseRowRawCsv(columns);
						if (product == null)
						{
							return null;
						}

						products.Add(product);
					}
				}
				catch (Exception e)
				{
					LastError = $"Исключение для {line} " + e.Message + e.StackTrace;
				}
			}

			return products;
		}

		/// <summary>
		/// Конвертирует строку из файла в товар.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private TradesoftProduct ParseRowRawCsv(string[] columns)
		{
			columns = columns.Select(x => x.Replace("#NULL!", "")).ToArray();

			TradesoftProduct product = new TradesoftProduct();
			Type type = typeof(TradesoftProduct);

			//Цикл по свойствам сущности.
			for (int i = 0; i < _productModelPropertyNames.Length; i++)
			{
				if (i + 1 > columns.Length) continue; //The source file contains less than the result.

				//Получаем значение для данного свойства. 
				string cellText = columns[i];

				if (string.IsNullOrEmpty(cellText)) continue; //Ячейка не заполнена.

				//Получаем свойство по имени.
				PropertyInfo property = type.GetProperty(_productModelPropertyNames[i]);
				if (property != null)
				{
					object propertyValue = property.GetValue(product);


					//Для свойств ссылочного типа обязательно значение отличное от null. Иначе исключение.
					object value = TypesConverter.ConvertTypes(propertyValue, cellText);
					if (value == null)
					{
						//Так поступает библиотека обработки заполняя пустые значения.
						if (cellText.Contains("#NULL!"))
						{
							LastError = "Измените Excel file, разрешите редактирование и сохраните.";
							return null;
						}

						LastError =
							$"Не удалось преобразовать {cellText} в тип данных {property.PropertyType.Name} для {product.Name}.";
						return null;
					}

					property.SetValue(product, value, null);
				}
			}

			return product;
		}

		private void FilterProducts(List<TradesoftProduct> products)
		{
			foreach (var product in products)
			{
				product.Name = product.Name.Replace("\"", "")
					.Replace("\t", "").Replace("   ", " ")
					.Replace("  "," ");// For old 1C.

			}
		}

		/// <summary>
		/// Based on the name of the product get it brand.
		/// </summary>
		private void GetBrand(List<TradesoftProduct> products)
		{
			foreach (var product in products)
			{
				string name = product.Name.Trim();
				string[] data = name.Split(' ');

				if (data.Length < 2) continue;

				product.Brand = data[1];


				if (product.Brand.Length > 1 && product.Brand.Substring(0, 1) == "(")
				{
					product.Brand = "ГОСТ";
				}
			}
		}

		private void GetBearingSizes(List<TradesoftProduct> products)
		{
			foreach (var product in products)
			{
				string name = product.Name.Trim();
				string[] data = name.Split(' ');

				if (data.Length < 2) continue;

				string sizesStr = data.FirstOrDefault(x => x.Contains('*'));
				if (sizesStr == null) continue;

				string[] sizesArr = sizesStr.Split('*');
				if (sizesArr.Length < 3) continue;

				//Сorrecting the error of inserting a date instead of a string in xls
				for (int i = 0; i < sizesArr.Length; i++)
				{
				   sizesArr[i] = sizesArr[i].Replace("(", "").Replace(")", "");
				}

				product.Vnutr = sizesArr[0];
				product.Nar = sizesArr[1];
				product.Shirina = sizesArr[2];
			}
		}

		/// <summary>
		/// Get import name from name column.
		/// </summary>
		/// <param name="products"></param>
		private void GetImportBearingName(List<TradesoftProduct> products)
		{
			foreach (var product in products)
			{
				string name = product.Name.Trim();
				string[] data = name.Split(' ');

				if (data.Length == 0) continue;

				product.ImportBearingName = data[0];
			}
		}

		private bool SaveResultFile(List<TradesoftProduct> products, string fileName)
		{
			WorkBook workBook = WorkBook.Create(ExcelFileFormat.XLS); //Not work xlsx format.

			// Create a blank WorkSheet
			WorkSheet workSheet = workBook.CreateWorkSheet("List1");

			//Add title.
			for (int k = 0; k < _resultFileColumns.Length; k++)
			{
				// Add data and styles to the new worksheet
				workSheet[$"{_lettersArray[k]}1"].Value = _resultFileColumns[k];
			}

			int row = 2;
			foreach (var product in products)
			{
				for (int k = 0; k < _resultFileColumns.Length; k++)
				{
					PropertyInfo property = product.GetType().GetProperty(_resultFileColumns[k]);
					object propertyValue = property == null ? null : property.GetValue(product);

					// Add data and styles to the new worksheet
					workSheet[$"{_lettersArray[k]}{row}"].StringValue = (string)propertyValue;

				}

				row++;
			}

			workBook.SaveAs(fileName);

			return true;
		}

		private void SaveToCsv(List<TradesoftProduct> products, string fileName)
		{
			char separator = ';';

			using (Stream stream = File.OpenWrite(fileName))
			using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
			{
				List<string> title = new List<string>();
				//Add title.
				for (int k = 0; k < _resultFileColumns.Length; k++)
				{
					title.Add(_resultFileColumns[k]);
				}

				string text = string.Join(separator, title);
				writer.WriteLine(text);


				foreach (var item in products)
				{
					List<string> info = new List<string>();
					info.Add(item.Brand);
					info.Add(item.Name);
					info.Add(item.Article);
					info.Add(item.Vnutr);
					info.Add(item.Nar);
					info.Add(item.Shirina);
					info.Add(item.ImportBearingName);

					text = string.Join(separator, info);
					writer.WriteLine(text);
				}
			}
		}
	}
}
