using Microsoft.EntityFrameworkCore;

namespace GearShop.Models.Entities
{
	/// <summary>
	/// Хранит информацию о процессе синхронизации данных. Для прогресс бара.
	/// </summary>
	[Keyless]
	public class PriceSynchronizeStatus
	{
		public int Current { get; set; }
		public int Total { get; set; }
	}
}
