using GearShop.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using GearShop.Services.Repository.Entities;
using System.Reflection.Metadata;
using Microsoft.Identity.Client;
using Microsoft.Data.SqlClient;

namespace GearShop.Services.Repository
{
    public class GearShopDbContext : DbContext, IGearShopDbContext
    {
        public GearShopDbContext(DbContextOptions<GearShopDbContext> options) : base(options)
        {

        }
        
        /// <summary>
        /// Пользователи в системе.
        /// </summary>
        public DbSet<Users> Users { get; set; }
        
        public string GetUserGroupRole(string userName)
        {
           return this.Database.SqlQueryRaw<string>("SELECT [dbo].GetUserGroupRole(@userName) as value",
                new SqlParameter("userName", userName)).FirstOrDefault();
        }
    }
}
