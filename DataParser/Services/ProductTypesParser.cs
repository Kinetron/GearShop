using DataParser.Contracts;
using DataParser.Models;
using DataParser.Models.Products;
using DataParser.ProductTypeAnalyzers;
using DataParser.ProductTypeParsers;

namespace DataParser.Services
{
    /// <summary>
    /// Разбирает строку из названия продукта на данные сущностей. 
    /// </summary>
    public class ProductTypesParser
    {
        /// <summary>
        /// Анализаторы типов продуктов. Порядок анализаторов должен соответствовать
        /// порядку сортировки в прайс листе.
        /// </summary>
        private readonly IProductAnalyzer[] _analyzers =
        {
            new BearingAnalyzer(),
            new OuterHolderAnalyzer(),
            new SleeveAnalyzer(),
            new MudStripperAnalyzer(),
            new СhainLinkAnalyzer(),
            new RingAnalyzer(),
            new BoxAnalyzer(),
            new CuffAnalyzer()
        };

        /// <summary>
        /// Парсеры разных типов продуктов.
        /// </summary>
        private readonly IProductTypeParser[] _typeParsers;

        /// <summary>
        /// Продукты полученные в результате обработки данных.
        /// </summary>
        public AllProducts AllProducts { get; private set; } = new AllProducts();

        public ProductTypesParser()
        {
            _typeParsers = new IProductTypeParser[]
            {
                new BearingParser(AllProducts.Bearings),
                new OuterHolderParser(AllProducts.OuterHolders),
                new SleeveParser(AllProducts.Sleeves),
                new MudStripperParser(AllProducts.MudStrippers),
                new СhainLinkParser(AllProducts.СhainLinks),
                new RingParser(AllProducts.Rings),
                new BoxParser(AllProducts.Boxes),
                new CuffParser(AllProducts.Cuffs)
            };
            
            if (_typeParsers.Length != _analyzers.Length)
            {
                throw new ArgumentException("Неверное определение парсеров и анализаторов.");
            }
        }

        /// <summary>
        /// Разбирает список продуктов на сущности.
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        public bool ParseProducts(List<Product> products)
        {
            int analyzerPos = 0;

            for(int i = 0; i < products.Count; i++)
            {
                if(analyzerPos >= _analyzers.Length) break;

                //Так как товары отсортированы по порядку, упрощаем анализ.
                //Если находим товар который не соответствует текущей операции в конвейере, переключаем операцию.
               
                bool typeNotChange = _analyzers[analyzerPos].Analyze(products[i].Name);
                if (!typeNotChange)
                {
                    i--;
                    analyzerPos++;
                    continue;
                }

                _typeParsers[analyzerPos].Parse(products[i]);
            }

            return true;
        }
    }
}
