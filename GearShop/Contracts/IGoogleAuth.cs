using System.IdentityModel.Tokens.Jwt;
using GearShop.Models.Dto.Authentication;

namespace GearShop.Contracts;

public interface IGoogleAuth
{
    Task<string> Authorization(string token);

    AccountInfoDto GetUserInfoFromJwt(JwtSecurityToken token);
}