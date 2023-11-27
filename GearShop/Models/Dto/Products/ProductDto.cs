using Newtonsoft.Json;

namespace GearShop.Models.Dto.Products
{
    public class ProductDto
    {
		public int Id { get; set; }

		/// <summary>
		/// Тип продукта.
		/// </summary>
		public int ProductTypeId { get; set; }

		/// <summary>
		/// Наименование товара.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Количество.
		/// </summary>
		public int Amount { get; set; }

		/// <summary>
		/// Цена.
		/// </summary>
		/// 
		public decimal Cost { get; set; }

        /// <summary>
        /// Картинка товара.
        /// </summary>
        public string ImageName { get; set; } = "NoPhoto.png";
    }
}
