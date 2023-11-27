using Newtonsoft.Json;

namespace GearShop.Models.Dto
{
	public class OrderItemDto
	{
		/// <summary>
		/// Строка дерева primeui.
		/// </summary>
		[JsonProperty("data")]
		public OrderItemDto Item { get; set; }
	}
}
