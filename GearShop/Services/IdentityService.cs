using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using GearShop.Models;
using GearShop.Contracts;
using Azure.Core;
using GearShop.Services.Repository;

namespace GearShop.Services
{
    /// <summary>
    /// Авторизация с использованием jwt. Тектовый пароль //HBxD7sGFFttHxKw
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private readonly string _jwtKey;
        private readonly IGearShopRepository _repository;
        public string LastError { get; private set; }

        public IdentityService(string jwtKey, IGearShopRepository repository)
        {
            _jwtKey = jwtKey;
            _repository = repository;
        }

        /// <summary>
        /// Создает токен авторизации.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string CreateToken(string userName, string role)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
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
        
        /// <summary>
        /// Проверят пользователя на возможность входа в систему. Возвращает токен для авторизации.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Login(string userName, string password)
        {
            //Проверка наличия пользователя в БД, получение хэшсоли для его пароля.
            string dbHashSalt = _repository.GetUserHashSalt(userName);

            if (dbHashSalt == null)
            {
                LastError = "Неверное имя пользователя или пароль.";   
                return null;
            }

            //Неверный пароль.
            if (!BCrypt.Net.BCrypt.Verify(password, dbHashSalt))
            {
                LastError = "Неверное имя пользователя или пароль.";
                return null;
            }

            //Получение роли.
            string role = _repository.GetUserGroupCode(userName);

            return CreateToken(userName, role);
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
