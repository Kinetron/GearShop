using Microsoft.Build.Framework;

namespace GearShop.Models.Dto.Authentication
{
    /// <summary>
    /// Информация для аутентификации пользователя.
    /// </summary>
    public class AuthData
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
