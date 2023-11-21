namespace GearShop.Models
{
	/// <summary>
	/// Информация о заказе.
	/// Странный вид модели связан с особенностью puitreetable в primeui.
	/// Компонент не умеет отображать разные модели внутри дерева.
	/// </summary>
	public class OrderItemDto
	{
		public long OrderId { get; set; }

		//public string OrderDate {}
		public string BuyerName { get; set; }
		public DateTime Created { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }

		/// <summary>
		/// Стоимость 1 шт.
		/// </summary>
		public decimal? Amount { get; set; }

		/// <summary>
		/// Общая сумма заказа.
		/// </summary>
		public decimal TotalSum { get; set; }
		public string BuyerPhone { get; set; }
	}
}