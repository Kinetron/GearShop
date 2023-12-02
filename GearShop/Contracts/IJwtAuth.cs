using System.IdentityModel.Tokens.Jwt;
using GearShop.Models.Dto.Authentication;

namespace GearShop.Contracts;

public interface IJwtAuth
{
    /// <summary>
    /// Создает токен авторизации.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    string CreateToken(string userName, string role, string email, string pictureUrl, string userId);

    AccountInfoDto GetUserInfoFromJwt(JwtSecurityToken token);
}