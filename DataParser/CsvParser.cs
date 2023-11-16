using DataParser.Helpers;
using DataParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataParser
{
    /// <summary>
    /// Конвертирует данные из csv файла в список продуктов.
    /// </summary>
    public class CsvParser
    {
        /// <summary>
        /// Имена свойств заполняемой модели из строки файла.
        /// Последовательность названий должна соответствовать последовательности ячеек в файле.
        /// </summary>
        private readonly string[] _productModelPropertyNames =
        {
            "Name", "ImageName", "PurchaseCost", "RetailCost", "WholesaleCost", "Rest", "Available"
        };

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Обрабатывает файл. 12 000 строк обрабатываются почти мгновенно.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<Product> ParseFile(string filePath, char separator)
        {
            List<Product> products = new List<Product>();

            using (StreamReader file = new StreamReader(filePath))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    string[] columns = line.Split(separator);

                    Product product = ParseRow(columns);
                    if (product == null)
                    {
                        return null;
                    }

                    products.Add(product);
                }
            }

            return products;
        }

        /// <summary>
        /// Конвертирует строку из файла в товар.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Product ParseRow(string[] columns)
        {
            Product product = new Product();
            Type type = typeof(Product);

            for (int i = 0; i < columns.Length; i++)
            {
                string cellText = columns[i];
                PropertyInfo property = type.GetProperty(_productModelPropertyNames[i]);
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
    }
}
