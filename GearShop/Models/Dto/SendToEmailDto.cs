namespace GearShop.Models.Dto
{
	/// <summary>
	/// Данные формы «Написать нам».
	/// </summary>
	public class SendToEmailDto
	{
		public string Name { get; set; }
		public string Email { get; set; }

		public string Text { get; set; }
	}
}
