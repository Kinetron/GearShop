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

		/// <summary>
		/// Не зарегистрированые покупатели.
		/// </summary>
		public DbSet<NonRegisteredBuyer> NonRegisteredBuyers { get; set; }

        public string GetUserGroupRole(string userName)
        {
           return this.Database.SqlQueryRaw<string>("SELECT [dbo].GetUserGroupRole(@userName) as value",
                new SqlParameter("userName", userName)).FirstOrDefault();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
	        modelBuilder.Entity<NonRegisteredBuyer>(entity =>
	        {
		        entity.HasKey(e => e.ClusterId);

		        entity.ToTable(tb =>
		        {
			        tb.HasTrigger("tr_NonRegisteredBuyers_LogIns");
			        tb.HasTrigger("tr_NonRegisteredBuyers_LogUpd");
		        });
	        });
        }
	}
}
