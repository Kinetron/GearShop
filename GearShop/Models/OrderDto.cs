using GearShop.Models.Dto;
using Newtonsoft.Json;

namespace GearShop.Models
{
	/// <summary>
	/// Список заказов.
	/// </summary>
	public class OrderDto
	{
		/// <summary>
		/// Заголовки строк дерева primeui.
		/// </summary>
		[JsonProperty("data")]
		public OrderItemDto Title { get; set; }
		
		/// <summary>
		/// Содержимое деревьев.
		/// </summary>
		[JsonProperty("children")]
		public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
	}
}
