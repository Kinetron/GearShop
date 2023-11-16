using GearShop.Services.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearShop.Contracts;

public interface IGearShopDbContext
{
    /// <summary>
    /// Пользователи в системе.
    /// </summary>
    DbSet<Users> Users { get; set; }

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    string GetUserGroupRole(string userName);
}