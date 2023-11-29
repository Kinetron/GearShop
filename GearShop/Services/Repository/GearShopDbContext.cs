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
		/// Типы продуктов.
		/// </summary>
        public DbSet<ProductType> ProductTypes { get; set; }

		/// <summary>
		/// Не зарегистрированые покупатели.
		/// </summary>
		public DbSet<NonRegisteredBuyer> NonRegisteredBuyers { get; set; }

		/// <summary>
		/// Продукты в заказе.
		/// </summary>
		public DbSet<OrderItem> OrderItems { get; set; }

		/// <summary>
		/// Заказы клиента.
		/// </summary>
		public DbSet<Order> Orders { get; set; }

		/// <summary>
		/// Дополнительная информация о заказе.
		/// </summary>
		public DbSet<OrderInfo> OrderInfo { get; set; }
		
		public DbSet<SlaiderMainPage> SlaiderMainPage { get; set; }

		/// <summary>
		/// Источник данных сведений о продукте(Из прайса, добавлен на сайте)
		/// </summary>
		public DbSet<InfoSource> InfoSource { get; set; }

		/// <summary>
		/// Хранит информацию о процессе синхронизации данных. Для прогресс бара.
		/// </summary>
		public DbSet<PriceSynchronizeStatus> PriceSynchronizeStatus { get; set; }
		
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

			modelBuilder.Entity<Order>(entity =>
			{
				entity.ToTable(tb =>
				{
					tb.HasTrigger("tr_Orders_LogIns");
					tb.HasTrigger("tr_Orders_LogUpd");
				});
			});

			modelBuilder.Entity<OrderItem>(entity =>
			{
				entity.ToTable(tb =>
				{
					tb.HasTrigger("tr_OrderItems_LogIns");
					tb.HasTrigger("tr_OrderItems_LogUpd");
				});
			});

			modelBuilder.Entity<InfoSource>(entity =>
			{
				entity.ToTable(tb =>
				{
					tb.HasTrigger("tr_InfoSource_LogIns");
					tb.HasTrigger("tr_InfoSource_LogUpd");
				});
			});

			//modelBuilder.Entity<PriceSynchronizeStatus>(builder => { builder.HasNoKey(); });
		}
    }
}
