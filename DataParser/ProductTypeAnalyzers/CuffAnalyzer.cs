using DataParser.Contracts;

namespace DataParser.ProductTypeAnalyzers
{
    public class CuffAnalyzer : IProductAnalyzer
    {
        public bool Analyze(string productName)
        {
            //В прайсе товар отсортирован. Делаем упрощенную реализацию анализатора. Для ускорения работы.
            string[] notСontain = { "Ремень" }; //Набор

            return !notСontain.Any(x => productName.Contains(x));
        }
    }
}
