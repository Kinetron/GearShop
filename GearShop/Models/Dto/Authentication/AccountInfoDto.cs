namespace GearShop.Models.Dto.Authentication
{
	/// <summary>
	/// Информация об авторизированном в системе пользователе.
	/// </summary>
	public class AccountInfoDto
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string PictureUrl { get; set; }
		public bool IsAuth { get; set; }
		public bool IsAdmin { get; set; }
	}
}