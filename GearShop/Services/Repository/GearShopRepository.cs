﻿using GearShop.Contracts;
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
using Newtonsoft.Json;
using Serilog;
using OrderItemDto = GearShop.Models.OrderItemDto;
using System.Net;
using Product = GearShop.Models.Entities.Product;

namespace GearShop.Services.Repository
{
    /// <summary>
    /// Слой доступа к данным в БД.
    /// </summary>
    public class GearShopRepository : IGearShopRepository
    {
        private readonly GearShopDbContext _dbContext;

        /// <summary>
        /// Название источника в таблице InfoSource для добавления данных из web.
        /// </summary>
        private const string WebSourceName = "Добавлен с сайта";


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
        public List<ProductDto> GetProducts(int currentPage, int itemsPerPage, string searchText, int productTypeId, bool available)
        {
	        var data = _dbContext.Products.Where(x=>x.Deleted == 0);

	        if (available)
	        {
				data = data.Where(x => x.Rest > 0);
			}

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
		public int GetProductCount(string searchText, int productTypeId, bool available)
        {
			var data = _dbContext.Products.Where(x => x.Deleted == 0);

			if (available)
			{
				data = data.Where(x => x.Rest > 0);
			}

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
		/// Получает список всех продуктов на складе.
		/// </summary>
		/// <returns></returns>
		public List<ProductDto> GetProductsFromStockroom(int currentPage, int itemsPerPage, string searchText, int productTypeId, bool available)
		{
			var data = _dbContext.Products.Where(x => x.Deleted == 0);

			if (available)
			{
				data = data.Where(x => x.Rest > 0);
			}

			if (productTypeId > 0)
			{
				data = data.Where(x => x.ProductTypeId == productTypeId);
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
						ProductTypeId = product.ProductTypeId,
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
				object data = new { guid, ipAddress };
				Log.Error($"Ошибка {JsonConvert.SerializeObject(data)}", ex);
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
		public async Task<long> CreateOrder(List<ProductDto> model, OrderInfo orderInfo, string guid, string ip)
		{
			Guid userGuid = Guid.Parse(guid);

			//Нет такого покупателя.
			long? buyerId = _dbContext.NonRegisteredBuyers.FirstOrDefault(x => x.BuyerGuid == userGuid)?.ClusterId;
			if (buyerId == null)
			{
				//Добавим возможно пришел со старой версии или другого сервера.
				bool result = await SynchronizeNoRegUserGuidAsync(guid, ip);
				if (!result) return -1;

				buyerId = _dbContext.NonRegisteredBuyers.FirstOrDefault(x => x.BuyerGuid == userGuid)?.ClusterId;
			}

			if (buyerId == null)
			{
				Log.Error("Ошибка сохранения заказа. Повторно не получен guid пользователя.");
				return -1;
			}


			//Продукты в заказе.
			List<OrderItem> orderItems = new List<OrderItem>();
			foreach (var product in model)
			{
				//Ситуация на случай новой БД.
				var existProduct = await _dbContext.Products.FirstOrDefaultAsync(p=>p.Id == product.Id || p.Name == product.Name);
				if (existProduct == null)
				{
					Log.Warning($"Продукт {JsonConvert.SerializeObject(product)} есть в корзине.{Environment.NewLine} Но нет в БД. Удалим из заказа.");
					continue;
				}

				OrderItem item = new OrderItem()
				{
					ProductId = existProduct.Id,
					Amount = product.Amount
				};

				orderItems.Add(item);
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
					foreach (var item in orderItems)
					{
						item.OrderId = order.Id;
						await _dbContext.OrderItems.AddAsync(item);
					}

					await _dbContext.SaveChangesAsync();

					orderInfo.OrderId = order.Id;
					await _dbContext.OrderInfo.AddAsync(orderInfo);
					await _dbContext.SaveChangesAsync();

					await transaction.CommitAsync();

					Log.Information($"Create new order from {ip} by {userGuid}");
					return await Task.FromResult(order.Id);
				}
				catch (Exception e)
				{
					transaction.Rollback();
					object data = new { model, orderInfo, guid };
					string json = JsonConvert.SerializeObject(data, Formatting.Indented);
					Log.Error($"Ошибка {json}", e);
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
			return await _dbContext.ProductTypes.OrderBy(x=>x.Name).ToListAsync();
		}

		/// <summary>
		/// Возвращает название картинок и название продукта к которому относиться картинка.
		/// </summary>
		/// <returns></returns>
		public async Task<List<KeyValuePair<string, string>>> GetProductImagesInfoAsync()
		{
			return await _dbContext.Products.Where(p=>!string.IsNullOrEmpty(p.ImageName)).Select(p => new KeyValuePair<string, string>(p.Name, p.ImageName)
			).ToListAsync();
		}

		/// <summary>
		/// Добавляет продукт в прайс.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<bool> CreateProductAsync(ProductDto model)
		{
			//Существует ли еще тип продукта в БД?
			var type = await _dbContext.ProductTypes.FirstOrDefaultAsync(t=>t.Id == model.ProductTypeId);

			if (type == null)
			{
				Log.Error($"Ошибка. Не найден тип продукта с id={model.ProductTypeId}");
				return false;
			}

			//Идентификатор источника данных
			var infoSourceId = await _dbContext.InfoSource.FirstOrDefaultAsync(x => x.Name == WebSourceName);
			if (infoSourceId == null)
			{
				Log.Error($"Not found id for InfoSource with name {WebSourceName}");
				return false;
			}
			
			try
			{
               Product product = new Product()
				{
					Name = model.Name,
					RetailCost = model.Cost,
					ImageName = model.ImageName,
					Rest = model.Amount,
					ProductTypeId = model.ProductTypeId,
					InfoSourceId = infoSourceId.Id,
				};

               await _dbContext.Products.AddAsync(product);
               await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Обновляет информацию о продукте в прайсе.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<bool> UpdateProductAsync(ProductDto model)
		{
			//Существует ли еще тип продукта в БД?
			var type = await _dbContext.ProductTypes.FirstOrDefaultAsync(t => t.Id == model.ProductTypeId);

			if (type == null)
			{
				Log.Error($"Ошибка. Не найден тип продукта с id={model.ProductTypeId}");
				return false;
			}

			try
			{
				var product = await _dbContext.Products.FirstOrDefaultAsync(p=>p.Id == model.Id);
				if (product == null)
				{
					Log.Error($"Ошибка. Не найден продукт с id={model.Id}");
					return false;
				}


				product.Name = model.Name;
				product.RetailCost = model.Cost;
				product.ImageName = model.ImageName;
				product.Rest = model.Amount;
				product.ProductTypeId = model.ProductTypeId;
				
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex);
				return false;
			}

			return true;
		}

		public async Task<bool> DeleteProductAsync(int id)
		{
			try
			{
				var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
				if (product == null)
				{
					Log.Error($"Ошибка. Не найден продукт с id={id}");
					return false;
				}

				product.Deleted = 1;
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex);
				return false;
			}

			return true;
		}
    }
}
