using GearShop.Contracts;
using GearShop.Models;
using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using DataParser.Models;
using GearShop.Models.Dto;
using OrderItemDto = GearShop.Models.OrderItemDto;

namespace GearShop.Services.Repository
{
    /// <summary>
    /// Слой доступа к данным в БД.
    /// </summary>
    public class GearShopRepository : IGearShopRepository
    {
        private readonly GearShopDbContext _dbContext;

        public GearShopRepository(GearShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Возвращает для пользователя хешированный пароль с солью. Если пользователя нет = null.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetUserHashSalt(string userName)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Name == userName && u.Deleted == 0)?.HashSalt;
        }
        
        /// <summary>
        /// Существует ли пользователь в системе?
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passwordHashSalt"></param>
        /// <returns></returns>
        public bool HasUser(string userName, string passwordHashSalt)
        {
            return _dbContext.Users.Any(u=>u.Name == userName && u.HashSalt == passwordHashSalt);
        }

        /// <summary>
        /// Возвращает код группы, используемый для разграничения прав.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetUserGroupCode(string userName)
        {
            return _dbContext.GetUserGroupRole(userName);
        }
		
        /// <summary>
        /// Получить список всех продуктов.
        /// </summary>
        /// <returns></returns>
        public List<ProductDto> GetProducts(int currentPage, int itemsPerPage, string searchText, int productTypeId)
        {
	        var data = _dbContext.Products.Where(x=>x.Rest > 0 && x.Deleted == 0);

	        if (productTypeId > 0)
	        {
		        data = data.Where(x=>x.ProductTypeId == productTypeId);
	        }
			
	        if (!string.IsNullOrEmpty(searchText))
	        {
		        data = data.Where(x => x.Name.Contains(searchText));
	        }

			//Переделать на нормальный sql, будет гораздо быстрее.

			return data.Select(product =>
				 new ProductDto()
					{
						Id = product.Id,
						Name = product.Name,
						Cost = product.RetailCost.Value,
						Amount = product.Rest.Value,
						ImageName = string.IsNullOrEmpty(product.ImageName) ? "NoPhoto.png" : product.ImageName
					}
				)
				   .Skip((currentPage - 1) * itemsPerPage)
				   .Take(itemsPerPage)
				   .ToList();
        }

		/// <summary>
		/// Возвращает количество продуктов.
		/// </summary>
		/// <returns></returns>
		public int GetProductCount(string searchText, int productTypeId)
        {
			var data = _dbContext.Products.Where(x => x.Rest > 0 && x.Deleted == 0);
			if (productTypeId > 0)
			{
				data = data.Where(x => x.ProductTypeId == productTypeId);
			}

			if (string.IsNullOrEmpty(searchText))
	        {
		        return data.Count();
			}
	        else
	        {
				return data.Where(x=>x.Name.Contains(searchText)).Count();
			}
        }

		/// <summary>
		/// Проверяет наличие guid в БД. Если нет – добавляет новую запись.
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="ipAddress"></param>
		/// <returns></returns>
		public async Task<bool> SynchronizeNoRegUserGuidAsync(string guid, string ipAddress)
		{
			try
			{
				Guid userGuid = Guid.Parse(guid);
				int count = await _dbContext.NonRegisteredBuyers.Where(u => u.BuyerGuid == userGuid).CountAsync();

				if (count == 0)
				{
					NonRegisteredBuyer buyer = new NonRegisteredBuyer()
					{
						BuyerGuid = userGuid,
						IpAddress = ipAddress
					};
					_dbContext.NonRegisteredBuyers.Add(buyer);
					await _dbContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				return await Task.FromResult(false);
			}
			
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Создает заказ. Возвращает номер заказа.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		public async Task<long> CreateOrder(List<ProductDto> model, OrderInfo orderInfo, string guid)
		{
			Guid userGuid = Guid.Parse(guid);

			//Нет такого покупателя.
			long? buyerId = _dbContext.NonRegisteredBuyers.First(x => x.BuyerGuid == userGuid)?.ClusterId;
			if (buyerId == null)
			{
				return -1;
			}
			
			using (var transaction = _dbContext.Database.BeginTransaction())
			{
				try
				{
					//Заказ клиента.
					Order order = new Order()
					{
						NonRegisteredBuyerId = buyerId.Value,
						TotalAmount = model.Sum(x => x.Amount * x.Cost)
					};
					await _dbContext.Orders.AddAsync(order);
					await _dbContext.SaveChangesAsync();
					
					//Продукты в заказе.
					foreach (var product in model)
					{
						OrderItem item = new OrderItem()
						{
							OrderId = order.Id,
							ProductId = product.Id,
							Amount = product.Amount
						};

						await _dbContext.OrderItems.AddAsync(item);
					}

					await _dbContext.SaveChangesAsync();

					orderInfo.OrderId = order.Id;
					await _dbContext.OrderInfo.AddAsync(orderInfo);
					await _dbContext.SaveChangesAsync();

					await transaction.CommitAsync();

					return await Task.FromResult(order.Id);
				}
				catch (Exception e)
				{
					transaction.Rollback();
					return await Task.FromResult(-1);
				}
			}
		}

		/// <summary>
		/// Заказы пользователя.
		/// </summary>
		/// <param name="noRegUseGuid"></param>
		/// <returns></returns>
		public async Task<List<OrderDto>> GetOrderList(string noRegUseGuid)
		{
			var items = await _dbContext.OrderItems 
				.Join(_dbContext.Products,
					items => items.ProductId, p=> p.Id, 
					(item, product)=> new OrderItemDto()
					{
						OrderId = item.OrderId,
						Quantity = item.Amount,
						Created = item.Created.Value,
						ProductName = product.Name,
						Amount = product.RetailCost.Value,
					}
				).ToListAsync();

			var orders = items.OrderByDescending(x=>x.OrderId).GroupBy(x => x.OrderId);

			List<OrderDto> orderTree = new List<OrderDto>(); //Заказы в виде дерева.

			foreach (var group in orders)
			{
				//Строка древовидного списка.
				OrderDto row = new OrderDto()
				{
					Title = new OrderItemDto()
					{
						OrderId = group.Key,
						Quantity = group.Count()
					}
				};

				//Дети внутри раскрывающейся строки дерева.
				List<OrderDto> children = new List<OrderDto>();
				row.Orders = children;

				foreach (var item in group)
				{
					OrderItemDto child = new OrderItemDto()
					{
						OrderId = item.OrderId,
						ProductName = item.ProductName,
						Quantity = item.Quantity,
						Amount = item.Amount,
						Created = item.Created,
					};

					row.Title.Created = item.Created;

					children.Add(new OrderDto()
					{
						Title = child
					});
				}

				//Дополнительная информация о заказе.
				OrderInfo info = _dbContext.OrderInfo.First(x => x.OrderId == row.Title.OrderId);
				row.Title.BuyerName = info.BuyerName;
				row.Title.BuyerPhone = info.BuyerPhone;

				Order order = _dbContext.Orders.First(x=>x.Id == row.Title.OrderId);
				row.Title.TotalSum = order.TotalAmount;

				orderTree.Add(row);
			}

			return await Task.FromResult(orderTree);
		}

		/// <summary>
		/// Данные для отображения слайдера на главной странице.
		/// </summary>
		/// <returns></returns>
		public async Task<List<SlaiderMainPageDto>> MainPageSlaiderDataAsync()
		{
			return await _dbContext.SlaiderMainPage.Select(data => new SlaiderMainPageDto()
			{
				Title = data.Title,
				Description = data.Description,
				FileName = data.FileName
			}).ToListAsync();
		}

		/// <summary>
		/// Возвращает список продуктов.
		/// </summary>
		/// <returns></returns>
		public async Task<List<ProductType>> GetProductTypesAsync()
		{
			return await _dbContext.ProductTypes.ToListAsync();
		}
    }
}
