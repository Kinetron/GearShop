using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataParser.Models;
using IronXL;
using DataParser.Helpers;
using IronXL.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace DataParser
{
    /// <summary>
    /// Парсер списка товаров выгруженных из 1С версии 8.3.23(конфиг 2.5.14.82) в формате Excel.
    /// Предназначен для разборки данных из прайса списка всех товаров,
    /// выгружаемых менеджером магазина. 
    /// Разбирает товары из общего списка на группы(сальник, манжета и т.п.).
    /// </summary>
    public class Excel1сShopParser2022Format
    {
	    private WorkBook _workBook = null;

        /// <summary>
        /// Путь к обрабатываемому файлу.
        /// </summary>
	    private string _workBookPath = null;

		/// <summary>
		/// Часть строки в первой ячейки прайс листа, на основании которой мы понимаем что нашли первый товар в списке.
		/// </summary>
		private const string BeginPriceLabel = "шт";

		/// <summary>
		/// Папка в которую сохраняются картинки из excel файла.
		/// </summary>
		public const string ImageFolder = "Images";

		/// <summary>
		/// Псевдо идентификаторы картинок(имена продуктов)
		/// id-название картинки без расширения, name - название продукта к которому привязана картинка.
		/// </summary>
		private List<IdName> _imageIds = new List<IdName>();

        /// <summary>
        /// Массив для определения буквы по индексу, нужен только для парсинга с .xls
        /// </summary>
        private readonly string[] _lettersArray =
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I",
            "J", "K", "L", "M", "N", "O", "P", "Q", "R",
            "S", "T", "U", "V", "W", "X", "Y", "Z", "AA",
            "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI",
            "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ",
            "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ"
        };

        /// <summary>
        /// Имена свойств заполняемой модели из строки Excel файла.
        /// Последовательность названий должна соответствовать последовательности ячеек в файле.
        /// </summary>
        private readonly string[] _productModelPropertyNames =
        {
            "Name", "ImageName", "PurchaseCost", "RetailCost", "WholesaleCost", "Rest", "Available"
        };

        /// <summary>
        /// Связь свойств из модели(_productModelPropertyNames) с номерами ячеек из excel файла.
        /// Для строк с картинками.
        /// </summary>
        private readonly int[] _sourceColumnAssociation =
        {
            3, 12, 13, 14, 15, 16, 17 //Номер ячейки в файле.
        };

        /// <summary>
        /// Счетчик для получения идентификаторов картинок.
        /// </summary>
        private int _imageIdCount;

		/// <summary>
		/// Номер ячейки содержащей название продукта.
		/// </summary>
		private readonly int _productNamePos = 3;

		/// <summary>
		/// Сообщение об ошибке.
		/// </summary>
		public string LastError { get; private set; }

		/// <summary>
		/// Возвращает следующий идентификатор картинки.
		/// </summary>
		/// <returns></returns>
		private int GetNextImageId()
		{
			return ++_imageIdCount;
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
		/// Задает идентификаторы продуктов к которым привязаны картинки.
		/// В качестве идентификатора выступает имя.
		/// В прайсе нет идентификаторов продукта и картинок.
		/// </summary>
		/// <param name=""></param>
		public void SetImageInfo(List<KeyValuePair<string, string>> info)
		{
			_imageIds = new List<IdName>();
            
			info.ForEach(x =>
            {
	            int index = x.Value.IndexOf(".");
	            string digStr = x.Value.Substring(0, index);

	            if (int.TryParse(digStr, out int dig))
	            {
                    _imageIds.Add(new IdName()
                    {
                        Name = x.Key,
                        Id = dig
                    });
	            }
            });

			//Получение текущего значение идентификатора картинки. Если не первый раз.
			if (info.Count > 0)
			{
				_imageIdCount = _imageIds.Max(x => x.Id);
			}
		}

        /// <summary>
        /// Парсит файл.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lineLimit"></param>
        /// <returns></returns>
        public List<Product> ParseFile(string filePath, int lineLimit = 0)
        {
            //20минут файл из 12000. Старая версия.
	        return ParseFile(filePath, (curent, total)=>{}, lineLimit);
        }

        /// <summary>
        /// Конвертирует файл. При необходимости можно ограничить размер результирующих данных.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lineLimit">размер результирующих данных</param>
        /// <returns></returns>
        public List<Product> ParseFile(string filePath, Action<int, int> progressInfo, int lineLimit = 0)
        {
            WorkBook workBook = null;

            // Считывание данных с файла по пути filePath
            try
            {
                workBook = WorkBook.Load(filePath);
            }
            catch (IOException ex)
            {
                LastError = $"Ошибка при чтении файла {Path.GetFileName(filePath)}.";
                return null;
            }
            catch (Exception ex)
            {
                LastError = $"Исключение: {ex.Message}";
                return null;
            }

            WorkSheet sheet = workBook.WorkSheets[0]; // Выбор первого листа
            
			int currentRowIndex = 1; //Индекс строки.
            string[] headers = null; //Заголовки всех колонок 

            (headers, currentRowIndex) = GetLastHeader(sheet);
            if (headers == null)
            {
                LastError = $"Ошибка. Не найден товар в файле {Path.GetFileName(filePath)}.";
                return null;
            }

            List<Product> products = new List<Product>();

            int totalRows = sheet.Rows.Count - (currentRowIndex + 1); //Всего строк в файле.
            if(lineLimit > 0) { totalRows  = lineLimit; }

            int currentRow = 1; //Текущая обработанная строка.

            while (currentRowIndex < sheet.Rows.Count)
            {
                currentRowIndex++;

                //Читаем все данные из строки.
                object[] data = new object[headers.Length];

                for (int i = 0; i < headers.Length; i++)
                {
                    data[i] = sheet[_lettersArray[i] + currentRowIndex.ToString()].Value;
                }

                try
                {
                    Product product = ParseRow(data);
                    if (product == null)
                    {
                        return null;
                    }

                    products.Add(product);
                }
                catch (Exception ex)
                {
                    LastError = $"Исключение: {ex.Message}";
                    return null;
                }
             
                progressInfo(currentRow, totalRows); //Передаем пользователю о прогрессе обработки строк.
                currentRow ++;

                if (lineLimit != 0 && currentRow > lineLimit) break;
            }

            progressInfo(totalRows, totalRows); //Обработка завершена.

            return products;
        }

        /// <summary>
        /// Возвращает последнюю строку с заголовком(заголовок может состоять из множества строк) и ее индекс.
        /// После заголовка идет строка с товаром.
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private (string[], int) GetLastHeader(WorkSheet sheet)
        {
            int currentRowIndex = 1;
            string[] headers = null; //Заголовки всех колонок 

            //Цикл работает до первого товара в прайс листе. 
            while (currentRowIndex < sheet.Rows.Count)
            {
                string[] titles =
                    sheet.Rows[currentRowIndex].StringValue.Split('\t'); // Получаем заголовки всех колонок
                if (titles.Length > 1 && titles.Any(x => x.Contains(BeginPriceLabel))) //Нашли товар. Заголовок закончился.
                {
                    return (headers, currentRowIndex);
                }

                headers = titles;
                currentRowIndex++; //Пропускаем заголовки.
            }

            return (null, 0);
        }

        /// <summary>
        /// Преобразовывает строку в данные о товаре.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Product ParseRow(object[] data)
        {
            Product product = new Product();
            Type type = typeof(Product);
            
            for (int propertyIndex = 0; propertyIndex < _productModelPropertyNames.Length; propertyIndex++)
            {
                string cellText = string.Empty;

                //Поиск первой заполненной ячейки. Название товара может быть не в первой ячейке, а в диапазоне 0-13.
                if (propertyIndex == 0)
                {
                    cellText = FindFirstTextInRow(0, data);
                }
                else
                {
                    cellText = data[_sourceColumnAssociation[propertyIndex] - 1].ToString();
                }

                PropertyInfo property = type.GetProperty(_productModelPropertyNames[propertyIndex]);
                if (property != null)
                {
                    object propertyValue = property.GetValue(product);

                    //Для свойств ссылочного типа обязательно значение отличное от null. Иначе исключение.
                    object value = TypesConverter.ConvertTypes(propertyValue, cellText);
                    if (value == null)
                    {
                        

                        LastError =
                            $"Не удалось преобразовать {cellText} в тип данных {property.PropertyType.Name} для {product.Name}.";

						return null;
                    }

                    property.SetValue(product, value, null);
                }
            }

            return product;
        }

        /// <summary>
        /// Ищет первую не пустую ячейку, возвращает ее содержимое.
        /// </summary>
        /// <param name="beginPos"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string FindFirstTextInRow(int beginPos, object[] data)
        {
            while (beginPos < data.Length)
            {
                string cellText = data[beginPos].ToString();

                //Пропуск ячеек не содержащих данные. Такие ячейки содержат значение 0.
                if ((int.TryParse(cellText, out int dig) && dig == 0))
                {
                    beginPos++;
                    continue;
                }
                
                return cellText;
            }

            return null;
        }

        /// <summary>
        /// Сохраняет полученные значения в файл.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePath"></param>
        public void SaveToСsvFile(List<Product> data, string filePath, string separator)
        {
            using (StreamWriter writer = File.CreateText(filePath))
            {
                foreach (var item in data)
                {
                    List<string> info = new List<string>();
                    info.Add(item.Name);
                    info.Add(item.ImageName);
                    info.Add(item.PurchaseCost.ToString());
                    info.Add(item.RetailCost.ToString());
                    info.Add(item.WholesaleCost.ToString());
                    info.Add(item.Rest.ToString());
                    info.Add(item.Available);
                    info.Add(item.ProductTypeName);

					string text = string.Join(separator, info);
                    writer.WriteLine(text);
                }
            }
        }

        /// <summary>
        /// Ускоренный алгоритм.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="progressInfo"></param>
        /// <param name="lineLimit"></param>
        /// <returns></returns>
        public List<Product> ParseFileFast(Action<int, int> progressInfo, int lineLimit = 0)
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
        private List<Product> ParseRawCsv(string filePath, char separator)
        {
	        List<Product> products = new List<Product>();

            bool isProductLine = false;

            using (StreamReader file = new StreamReader(filePath))
            {
	            string line = null;

	            try
	            {
		            while ((line = file.ReadLine()) != null)
		            {
                        if(string.IsNullOrEmpty(line)) break; //Конец файла.

			            string[] columns = line.Split(separator);

			            //Немного ускоряем работу игнорируя анализ строк.
			            if (!isProductLine && !IsProductLine(columns)) continue;
			            isProductLine = true;

			            //Пропуск строк содержащих картинки.
			            if (string.IsNullOrEmpty(columns[_sourceColumnAssociation[0]])) continue;


			            Product product = ParseRowRawCsv(columns);
			            if (product == null)
			            {
				            return null;
			            }

                        if(string.IsNullOrEmpty(product.Name)) continue;;

			            FilterProduct(product);
                        
                        //Для записей для которых есть картинки.
                        if (!string.IsNullOrEmpty(product.ImageName))
                        {
	                        var imgId = _imageIds.FirstOrDefault(x => x.Name == product.Name);

	                        if (imgId == null) //На сервере нет картинки.
							{
		                        //Добавляем данные для идентификации картинки при привязки к имени.
		                        var info = new IdName()
		                        {
			                        Id = GetNextImageId(),
			                        Name = product.Name,
		                        };

		                        _imageIds.Add(info);

		                        //Формируем название будущей картинки. Библиотека выгружает только png.
		                        product.ImageName = $"{info.Id}.png";
							}
	                        else
	                        {
								//Обязательно копируем название картинки с сервера. В прайсе нет идентификаторов.
								product.ImageName = $"{imgId.Id}.png";
							}
                        }
                        
			            products.Add(product);
		            }
	            }
	            catch (Exception e)
	            {
		            LastError = $"Исключение для {line} " +e.Message +e.StackTrace;
				}
            }

            return products;
        }

		/// <summary>
		/// Содержит ли данная строка информацию о товаре.
		/// </summary>
		/// <param name="columns"></param>
		/// <returns></returns>
		private bool IsProductLine(string[] columns)
        {
	        string productName = columns[_sourceColumnAssociation[0]];
            return productName.Contains(BeginPriceLabel);
        }

		/// <summary>
		/// Конвертирует строку из файла в товар.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private Product ParseRowRawCsv(string[] columns)
		{
			columns = columns.Select(x => x.Replace("#NULL!", "")).ToArray();

			Product product = new Product();
			Type type = typeof(Product);

			//Цикл по свойствам сущности.
			for (int i = 0; i < _productModelPropertyNames.Length; i++)
			{
				//Получаем значение для данного свойства. 
				string cellText = columns[_sourceColumnAssociation[i]];

                if(string.IsNullOrEmpty(cellText)) continue; //Ячейка не заполнена.

	            //Получаем свойство по имени.
				PropertyInfo property = type.GetProperty(_productModelPropertyNames[i]);
				if (property != null)
				{
					object propertyValue = property.GetValue(product);

					//Удаляем внутренние nbsp для сум.
					if (propertyValue.GetType() == typeof(decimal) || propertyValue.GetType() == typeof(int))
					{
						char nbsp = (char)160;
						cellText = cellText.Replace(nbsp, ' ').Replace(" ", "");
					}

                    //Rest float - contain point or dot.
					if (property.Name == "Rest" && (cellText.Contains(',') || cellText.Contains('.')))
					{
						cellText = cellText.Replace(',', '.');
                        int pointIndex = cellText.IndexOf('.');
                        cellText = cellText.Substring(0, pointIndex);
					}


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
                        
						if (property.Name != "Rest")
						{
							return null;
						}

						//Дробный остаток. 15,566 Новые изменения в б.д.
                        string[] str = cellText.Split(',');
                        cellText = str[0];
						value = TypesConverter.ConvertTypes(propertyValue, cellText);

						if (value == null)
						{
							LastError =
								$"Не удалось преобразовать {cellText} в тип данных {property.PropertyType.Name} для {product.Name}.";
							return null;
						}

					}

					property.SetValue(product, value, null);
				}
			}

			return product;
		}

		/// <summary>
		/// Удаляет лишние данные из модели
		/// </summary>
		private void FilterProduct(Product product)
		{
			product.Name = product.Name.Replace(", , шт", "");
		}

        /// <summary>
        /// Сохраняет картинки.
        /// </summary>
		public bool SaveImages(Action<int, int> progressInfo)
        {
	        LastError = string.Empty;

			if (!Directory.Exists(ImageFolder))
			{
				Directory.CreateDirectory(ImageFolder);
			}
			else
			{
				CleanDir(ImageFolder);
			}
			
			WorkSheet sheet = _workBook.WorkSheets[0]; // Выбор первого листа

            List<string> error = new List<string>();
			try
			{
				List<IronXL.Drawing.Images.IImage> images = sheet.Images;
				int totalRows = images.Count;
				int currentRow = 1; //Текущая обработанная строка.
                
				foreach (IronXL.Drawing.Images.IImage image in images)
				{
					Position position = image.Position;

					//На основании данных картинки, определяем имя продукта к которому она относиться.
					Product product = new Product();
					product.Name = GetProductName(sheet, position.Row2);
					FilterProduct(product);

					//Получаем идентификатор для продукта к которому привязана картинка
					var productId = _imageIds.FirstOrDefault(p => p.Name == product.Name);
					if (productId == null)
					{
						//Добавить обработку для странных имен " "" """
						string text = $"Не найден идентификатор для продукта {product.Name}. Картинки не будет.";
						error.Add(text);
						continue;
					}

					string imagePath = Path.Combine(ImageFolder, $"{productId.Id}.png");
					File.WriteAllBytes(imagePath, image.Data);
					ResizeImage(imagePath, 640, 480);
                    
					progressInfo(currentRow, totalRows); //Передаем пользователю о прогрессе обработки строк.
					currentRow++;
				}

				progressInfo(totalRows, totalRows);
			}
			catch (Exception e)
			{
                LastError = $"Возможно нужно открыть файл. Нажать <Разрешить редактирование>. Сохранить.  Исключение {e.Message}  {e.StackTrace}";
				return false;
			}
			
			//Удалить после доработок страных имен
			if (error.Any())
			{
				LastError = string.Join(Environment.NewLine, error);
			}

			return true;
		}

		/// <summary>
		/// Возвращает имя продукта из таблицы.
		/// </summary>
		/// <param name="sheet"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		private string GetProductName(WorkSheet sheet, int row)
        {
			return sheet[_lettersArray[_productNamePos] + row.ToString()].Value.ToString();
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

		/// <summary>
		/// Изменяет размер картинки.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void ResizeImage(string file, int width, int height)
        {
	        Image img = Image.Load(file);
	        img.Mutate(x => x.Resize(width, height));
	        img.Save(file);
		}
	}
}
