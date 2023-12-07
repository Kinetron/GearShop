using Newtonsoft.Json;

namespace GearShop.Models.Dto.Authentication
{
	/// <summary>
	/// Результат авторизации «В контакте».
	/// </summary>
	public class VkAuthDto
	{
		public string Uid { get; set; }
		
		[JsonProperty("first_name")]
		public string FirstName { get; set; }

		public string Photo { get; set; }
		public string Hash { get; set; }
	}
}
