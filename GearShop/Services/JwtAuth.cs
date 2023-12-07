using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GearShop.Services
{
    public class JwtAuth : IJwtAuth
	{
		private readonly IConfiguration _configuration;

		public JwtAuth(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// Создает токен авторизации.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public string CreateToken(string userName, string role, string email, string pictureUrl, string userId)
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, userName),
				new Claim(ClaimTypes.Role, role),
				new Claim(ClaimTypes.Email, email),
				new Claim("pictureUrl", pictureUrl)
			};

			string jwtKey = _configuration["JwtSettings:Key"];

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
																			   //Учетные данные.
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			//Создаем токен.
			var token = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.Now.AddHours(1),
				signingCredentials: creds //Учетные данные для подписи
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public AccountInfoDto GetUserInfoFromJwt(JwtSecurityToken token)
		{
			AccountInfoDto info = new AccountInfoDto();

			info.IsAuth = true;
			info.Name = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
			info.Email = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			info.PictureUrl = token.Claims.FirstOrDefault(c => c.Type == "pictureUrl")?.Value;

			string role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
			if (role == "Admin")
			{
				info.IsAdmin = true;
			}

			return info;
		}
	}
}
