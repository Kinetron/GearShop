using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using GearShop.Models;
using GearShop.Contracts;
using Azure.Core;
using GearShop.Services.Repository;
using GearShop.Models.Dto.Authentication;

namespace GearShop.Services
{
    /// <summary>
    /// Авторизация с использованием jwt.
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private readonly IGearShopRepository _repository;
        private readonly IJwtAuth _jwtAuth;

        public string LastError { get; private set; }

        public IdentityService(IGearShopRepository repository, IJwtAuth jwtAuth)
        {
	        _repository = repository;
	        _jwtAuth = jwtAuth;
        }
		
        /// <summary>
        /// Проверят пользователя на возможность входа в систему. Возвращает токен для авторизации.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Login(string userName, string password)
        {
	        if (!IsValidUser(userName, password)) return null;

            //Получение роли.
            string role = _repository.GetUserGroupCode(userName);

            return _jwtAuth.CreateToken(userName, role, "", "", "");
        }

		/// <summary>
		/// Проверяет существование пользователя.
		/// </summary>
		/// <returns></returns>
		public bool IsValidUser(string userName, string password)
        {
	        //Проверка наличия пользователя в БД, получение хэшсоли для его пароля.
	        string dbHashSalt = _repository.GetUserHashSalt(userName);

	        if (dbHashSalt == null)
	        {
		        LastError = "Неверное имя пользователя или пароль.";
		        return false;
	        }

	        //Неверный пароль.
	        if (!BCrypt.Net.BCrypt.Verify(password, dbHashSalt))
	        {
		        LastError = "Неверное имя пользователя или пароль.";
		        return false;
	        }

			return true;
        }
        
		//public void Register(UserDto request)
		//{
		//    //Создает хэшированный пароль с солью.
		//    string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

		//    user.Username = request.Username;
		//    user.PasswordHash = passwordHash;

		//}
	}
}
