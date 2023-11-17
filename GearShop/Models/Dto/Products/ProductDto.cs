namespace GearShop.Models.Dto.Products
{
    public class ProductDto
    {
        /// <summary>
        /// Наименование товара.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Количество.
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Цена.
        /// </summary>
        public decimal? Cost { get; set; }
    }
}
