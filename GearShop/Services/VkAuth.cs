using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using Newtonsoft.Json;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace GearShop.Services
{
    public class VkAuth : IVkAuth
	{
		private readonly IConfiguration _configuration;
		private readonly IJwtAuth _jwtAuth;

		public VkAuth(IConfiguration configuration, IJwtAuth jwtAuth)
		{
			_configuration = configuration;
			_jwtAuth = jwtAuth;
		}

		public async Task<string> Authorization(string token)
		{
			VkAuthDto data = JsonConvert.DeserializeObject<VkAuthDto>(token);

			string toHash = _configuration["VkAuth:AppId"] + data.Uid + _configuration["VkAuth:AppSecret"];
			string sign;
			using (var provider = MD5.Create())
			{
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(toHash);
				byte[] result = provider.ComputeHash(inputBytes);
				sign = Convert.ToHexString(result)
					.ToLower(); //md5 подпись от app_id+user_id+secret_key (согласно документации ВК)
			}

			if (data.Hash != sign) return null;

			//Создаем jwt токен для внешнего пользователя.
			return _jwtAuth.CreateToken(data.FirstName, "Сlient", "", data.Photo, data.Uid);
		}

		public AccountInfoDto GetUserInfo(string text)
		{
			AccountInfoDto info = new AccountInfoDto();
			
			VkAuthDto data = JsonConvert.DeserializeObject<VkAuthDto>(text);

			info.IsAuth = true;
			info.Name = data.FirstName;
			info.Email = "";
			info.PictureUrl = "images/vk.svg"; 

			return info;
		}
	}
}
