using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParser.Contracts;

namespace DataParser.ProductTypeAnalyzers
{
    /// <summary>
    /// Внешняя обойма
    /// </summary>
    internal class OuterHolderAnalyzer : IProductAnalyzer
    {
        public bool Analyze(string productName)
        {
            //В прайсе товар отсортирован. Делаем упрощенную реализацию анализатора. Для ускорения работы.
            string[] notСontain = { "Втулка" };

            return !notСontain.Any(x => productName.Contains(x));
        }
    }
}
