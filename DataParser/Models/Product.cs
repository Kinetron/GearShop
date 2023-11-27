namespace DataParser.Models
{
    /// <summary>
    /// Информация о товаре.
    /// Данные полученные из прайса, выгруженного менеджером из 1С.
    /// 
    /// Для свойств ссылочного типа обязательно значение отличное от null. 
    /// </summary>
    public class Product
    {
        public Product()
        {
            
        }

        public Product(Product product)
        {
            Name = product.Name;
            ImageName = product.ImageName;  
            PurchaseCost = product.PurchaseCost;
            RetailCost = product.RetailCost;
            Rest = product.Rest;
        }

        /// <summary>
        /// Полное наименование товара(у товара из прайса нет типа, только название).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Закупочная цена.
        /// </summary>
        public decimal PurchaseCost { get; set; }

        /// <summary>
        /// Розничная цена.
        /// </summary>
        public decimal RetailCost { get; set; }

        /// <summary>
        /// Оптовая цена.
        /// </summary>
        public decimal WholesaleCost { get; set; }

        /// <summary>
        /// Название картинки получаемой из прайса.
        /// </summary>
        public string ImageName { get; set; } = string.Empty;

        /// <summary>
        /// Остаток.
        /// </summary>
        public int Rest { get; set; }

        /// <summary>
        /// Наличие
        /// </summary>
        public string Available { get; set; } = string.Empty;

        /// <summary>
        /// Тип продукта - подшипник, втулка и т.п.
        /// </summary>
        public string ProductTypeName { get; set; } = string.Empty;
	}
}
