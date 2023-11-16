using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParser.Contracts;
using DataParser.Helpers;
using DataParser.Models;
using DataParser.Models.Products;

namespace DataParser.ProductTypeParsers
{
    internal class BearingParser : IProductTypeParser
    {
        private readonly List<Bearing> _bearing;

        /// <summary>
        ///  Сообщение об ошибке.
        /// </summary>
        public string LastError { get; private set; }

        public BearingParser(List<Bearing> bearing)
        {
            _bearing = bearing;
        }

        public bool Parse(Product data)
        {
            Bearing product = new Bearing(data);

            decimal? weight = GetWeight(product.Name);
            if (weight == null) return false;

            product.Weight = weight.Value;
            _bearing.Add(product);

            return true;
        }

        /// <summary>
        /// Получает вес из названия вида 1207   CX   (1207)   35*72*17   (0.32кг).
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private decimal? GetWeight(string data)
        {
            int kgPos = data.IndexOf("кг");
            if (kgPos == -1) return 0;

            string biginStr = data.Substring(0, kgPos).TrimEnd();
            string[] split = biginStr.Split(' ');

            string weightStr = split[split.Length - 1].Replace("(", "");

            decimal typeValue = 0; //Переменная нужна для определения типа через метод предназначенный для рефлексии.
            object value = TypesConverter.ConvertTypes(typeValue, weightStr);

            if (value == null)
            {
                LastError = $"Не удалось преобразовать {data} в тип данных decimal.";
                return null;
            }
            
            return (decimal)value;
        }
    }
}
