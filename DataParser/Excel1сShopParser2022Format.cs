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
        /// <summary>
        /// Часть строки в первой ячейки прайс листа, на основании которой мы понимаем что нашли первый товар в списке.
        /// </summary>
        private const string BeginPriceLabel = "шт";

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
        /// Алгоритм ищет первое не нулевое значение. Название товара может быть не в первой ячейке, а в диапазоне 0-13
        /// </summary>
        private readonly int[] _sourceColumnAssociation =
        {
            1, 14, 15, 16, 17, 18, 19 //Номер ячейки в файле.
        };
        
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; private set; }

        public List<ProductInfo> ParseFile(string filePath, int lineLimit = 0)
        {
            return ParseFile(filePath, (curent, total)=>{}, lineLimit);
        }

        /// <summary>
        /// Конвертирует файл. При необходимости можно ограничить размер результирующих данных.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lineLimit">размер результирующих данных</param>
        /// <returns></returns>
        public List<ProductInfo> ParseFile(string filePath, Action<int, int> progressInfo, int lineLimit = 0)
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

            List<ProductInfo> products = new List<ProductInfo>();

            int totalRows = sheet.Rows.Count - (currentRowIndex + 1); //Всего строк в файле.
            if(lineLimit > 0) { totalRows  = lineLimit; }

            int curentRow = 1; //Текущая обработанная строка.

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
                    ProductInfo productInfo = ParseRow(data);
                    if (productInfo == null)
                    {
                        return null;
                    }

                    products.Add(productInfo);
                }
                catch (Exception ex)
                {
                    LastError = $"Исключение: {ex.Message}";
                    return null;
                }
             
                progressInfo(curentRow, totalRows); //Передаем пользователю о прогрессе обработки строк.
                curentRow ++;

                if (lineLimit != 0 && curentRow > lineLimit) break;
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
        private ProductInfo ParseRow(object[] data)
        {
            ProductInfo product = new ProductInfo();
            Type type = typeof(ProductInfo);
            
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
        public void SaveToСsvFile(List<ProductInfo> data, string filePath, string separator)
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

                    string text = string.Join(separator, info);
                    writer.WriteLine(text);
                }
            }
        }
    }
}
