using DataParser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.ProductTypeAnalyzers
{
    internal class BoxAnalyzer : IProductAnalyzer
    {
        public bool Analyze(string productName)
        {
            //В прайсе товар отсортирован. Делаем упрощенную реализацию анализатора. Для ускорения работы.
            string[] notСontain = { "Комплект" };

            return !notСontain.Any(x => productName.Contains(x));
        }
    }
}
