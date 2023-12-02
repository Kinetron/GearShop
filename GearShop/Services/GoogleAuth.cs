using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using Google.Apis.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GearShop.Services
{
    /// <summary>
    /// Авторизация в системе с использованием токена полученного от Google
    /// https://accounts.google.com/gsi/client
    /// </summary>
    public class GoogleAuth : IGoogleAuth
	{
		private readonly IConfiguration _configuration;
		private readonly IJwtAuth _jwtAuth;

		public GoogleAuth(IConfiguration configuration, IJwtAuth jwtAuth)
		{
			_configuration = configuration;
			_jwtAuth = jwtAuth;
		}


		public async Task<string> Authorization(string token)
		{
			string clientId = _configuration["GoogleAuth:ClientId"];

			GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(token);

			if (!payload.Audience.Equals(clientId)) 
				return null;
				
			if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
				return null; 

			if (payload.ExpirationTimeSeconds == null)
				return null;
			else
			{
				DateTime now = DateTime.Now.ToUniversalTime();
				DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
				if (now > expiration)
				{
					return null;
				}
			}

			//Создаем jwt токен для внешнего пользователя.
			return _jwtAuth.CreateToken(payload.Name, "Сlient", payload.Email, payload.Picture, payload.JwtId);
		}
		public AccountInfoDto GetUserInfoFromJwt(JwtSecurityToken token)
		{
			AccountInfoDto info = new AccountInfoDto();

			info.IsAuth = true;
			info.Name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
			info.Email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			info.PictureUrl = token.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

			return info;
		}
	}
}
