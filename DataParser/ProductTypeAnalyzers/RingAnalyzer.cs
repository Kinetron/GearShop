using DataParser.Contracts;
namespace DataParser.ProductTypeAnalyzers
{
    internal class RingAnalyzer : IProductAnalyzer
    {
        public bool Analyze(string productName)
        {
            //В прайсе товар отсортирован. Делаем упрощенную реализацию анализатора. Для ускорения работы.
            string[] notСontain = { "Комплект" };

            return !notСontain.Any(x => productName.Contains(x));
        }
    }
}
