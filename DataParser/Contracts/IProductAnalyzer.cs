using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.Contracts
{
    /// <summary>
    /// Анализирует название товара, и определяет относится ли он к данному типу.
    /// </summary>
    internal interface IProductAnalyzer
    {
        /// <summary>
        /// Анализирует название товара, и определяет относится ли он к данному типу.
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        bool Analyze(string productName);
    }
}
