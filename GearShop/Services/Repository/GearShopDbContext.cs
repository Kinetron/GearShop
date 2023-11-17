using GearShop.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Metadata;
using Microsoft.Identity.Client;
using Microsoft.Data.SqlClient;
using GearShop.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace GearShop.Services.Repository
{
    public class GearShopDbContext : DbContext//, IGearShopDbContext
    {
        public GearShopDbContext(DbContextOptions<GearShopDbContext> options) : base(options)
        {

        }
        
        /// <summary>
        /// Пользователи в системе.
        /// </summary>
        public DbSet<Users> Users { get; set; }
        
        /// <summary>
        /// Все продукты.
        /// </summary>
        public DbSet<Product> Products { get; set; }

        public IDbContextTransaction BeginTransaction()
        {
	        return Database.BeginTransaction();
        }

        public void Commit()
        {
	        throw new NotImplementedException();
        }

        public string GetUserGroupRole(string userName)
        {
           return this.Database.SqlQueryRaw<string>("SELECT [dbo].GetUserGroupRole(@userName) as value",
                new SqlParameter("userName", userName)).FirstOrDefault();
        }
    }
}
