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


        /// <summary>
        /// Парсеры разных типов продуктов.
        /// </summary>
        private readonly IProductTypeParser[] _typeParsers;

        /// <summary>
        /// Продукты полученные в результате обработки данных.
        /// </summary>
        public AllProducts AllProducts { get; private set; } = new AllProducts();



		private readonly IProductAnalyzer[] _analyzers =
		{
			new OuterHolderAnalyzer(),


			new SleeveAnalyzer(),

			new MudStripperAnalyzer(),

			new СhainLinkAnalyzer(),

			new RingAnalyzer(),


			new BoxAnalyzer(),
			new CuffAnalyzer()
		};

		/// <summary>
		/// Ассоциации для определения типа товара.
		/// Первый столбец – тип товара.
		/// Второе значение – переключатель на новый товар. Если пуст – остановка обработки.
		/// </summary>
		private string[,] _productTypesAssociation = new string[,]
        {
	        {"Подшипники", "Внешняя обойма"},
	        {"Внешняя обойма", "Втулка"},
			{"Втулки", "Грязесъемник"},
			{"Грязесъемник", "Звено"},
			{"Звено", "Кольцо"},
			{"Кольца", "Комплект"},
			{"Корпуса","Манжета"},
			{"Манжеты", "Ремень"},
			{"Ремни", "Ремкомплект"},
			{"Ремкомплект","Ролик"},
			{"Сальники","Съёмник"},
			{"Съёмники","Уплотнение поршневое"},
			{"Уплотнители", "Цепь"},
			{"Цепи","Шайба"},
			{"Шайбы", "Шар"},
			{"Шары", "Шнур"},
			{"Шнуры", "Электрод"},
			{"Электроды", ""}
		};


		/// <summary>
		/// Определяет тип продукта.
		/// </summary>
		/// <param name="products"></param>
		/// <returns></returns>
		public bool DefineProductsType(List<Product> products)
		{
			int productTypePos = 0;
			int associationLen = _productTypesAssociation.GetLength(0) - 1;

			//В прайсе товар отсортирован. Делаем упрощенную реализацию анализатора. Для ускорения работы.
			foreach (var product in products)
	        {
		        string switchValue = _productTypesAssociation[productTypePos, 1];

		        //Товар изменился.
		        if (associationLen > productTypePos && product.Name.Contains(switchValue))
		        {
			        productTypePos++;
		        }

		        product.ProductTypeName = _productTypesAssociation[productTypePos, 0];
	        }

	        return true;
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
