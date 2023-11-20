using GearShop.Contracts;
using GearShop.Models;
using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;

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
            return _dbContext.Users.FirstOrDefault(u => u.Name == userName)?.HashSalt;
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
        public List<ProductDto> GetProducts(int currentPage, int itemsPerPage, string searchText)
        {
	        IQueryable<Product> data = _dbContext.Products;
	        if (!string.IsNullOrEmpty(searchText))
	        {
		        data = data.Where(x => x.Name.Contains(searchText));
	        }

	        return data
		        .Select(product =>
			        new ProductDto()
			        {
                        Id = product.Id,
				        Name = product.Name,
				        Cost = product.RetailCost,
				        Amount = product.Rest
			        })
		        .Skip((currentPage - 1) * itemsPerPage)
		        .Take(itemsPerPage)
		        .ToList();
        }

		/// <summary>
		/// Возвращает количество продуктов.
		/// </summary>
		/// <returns></returns>
		public int GetProductCount(string searchText)
        {
	        if (string.IsNullOrEmpty(searchText))
	        {
		        return _dbContext.Products.Count();
			}
	        else
	        {
				return _dbContext.Products.Where(x=>x.Name.Contains(searchText)).Count();
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
    }
}
