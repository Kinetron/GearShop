using GearShop.Models.Dto.Authentication;

namespace GearShop.Contracts;

public interface IVkAuth
{
    Task<string> Authorization(string token);
    AccountInfoDto GetUserInfo(string data);
}