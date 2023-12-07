namespace GearShop.Models.Entities
{
	/// <summary>
	/// Хранит информацию о процессе синхронизации данных. Для прогресс бара.
	/// </summary>
	public class PriceSynchronizeStatus
	{
		public int Id { get; set; }

		public int OperationId { get; set; }
		public int Current { get; set; }
		public int Total { get; set; }

		public string? ErrorText { get; set; }
		
		public string CurrentOperation { get; set; }

		public DateTime? BeginOperation { get; set; }
		public DateTime? EndOperation { get; set; }
	}
}
