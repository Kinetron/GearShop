namespace GearShop.Models.Entities
{
	/// <summary>
	/// Тип продукта.
	/// </summary>
	public class ProductType
	{
		public int Id { get; set; }

		public string Name { get; set; } = null!;

		public int SortingOrder { get; set; }

		public int Deleted { get; set; }
	}
}
