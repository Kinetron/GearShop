using GearShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GearShop.Contracts;

public interface IGearShopDbContext
{
    /// <summary>
    /// Пользователи в системе.
    /// </summary>
    DbSet<Users> Users { get; set; }

    /// <summary>
    /// Все продукты.
    /// </summary>
    DbSet<Product> Products { get; set; }
    
	/// <summary>
	/// Роль пользователя.
	/// </summary>
	/// <param name="userName"></param>
	/// <returns></returns>
	string GetUserGroupRole(string userName);
}