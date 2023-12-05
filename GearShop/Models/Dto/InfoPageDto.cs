namespace GearShop.Models.Dto
{
	/// <summary>
	/// Данные для формирования информационных страниц(«Статьи», «Новости», «Полезные советы»).
	/// </summary>
	public class InfoPageDto
	{
		public int Id { get; set; }
		public int ParentId { get; set; }

		public string Name { get; set; } = null!;

		public string? Title { get; set; }

		public string? TitleImage { get; set; }

		public string Content { get; set; } = null!;
	}
}
