using DataParser.Contracts;

namespace DataParser.ProductTypeAnalyzers
{
    /// <summary>
    /// По названию товара определяет является ли данный товар подшипником.
    /// </summary>
    internal class BearingAnalyzer : IProductAnalyzer
    {
        public bool Analyze(string productName)
        {
            //В прайсе товар отсортирован. Делаем упрощенную реализацию анализатора. Для ускорения работы.
            string[] notСontain = { "Внешняя обойма" };

            return !notСontain.Any(x => productName.Contains(x));
        }
    }
}
