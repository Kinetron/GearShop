using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParser.Models;

namespace DataParser.Contracts
{
    /// <summary>
    /// Разбирает название продукта на столбцы для сущности продукта. Продукты имеют разное количество параметров.
    /// </summary>
    internal interface IProductTypeParser
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Разбирает название продукта на столбцы для сущности продукта. Продукты имеют разное количество параметров.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Parse(Product data);
    }
}
