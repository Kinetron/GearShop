using System.IdentityModel.Tokens.Jwt;
using GearShop.Models;
using GearShop.Models.Dto.Authentication;

namespace GearShop.Contracts;

public interface IIdentityService
{
    /// <summary>
    /// Последнее сообщение об ошибке. В стиле windows api.
    /// </summary>
    string LastError { get; }

    /// <summary>
    /// Проверят пользователя на возможность входа в систему. Возвращает токен для авторизации.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    string Login(string userName, string password);

    /// <summary>
    /// Проверяет существование пользователя.
    /// </summary>
    /// <returns></returns>
    bool IsValidUser(string userName, string password);
}