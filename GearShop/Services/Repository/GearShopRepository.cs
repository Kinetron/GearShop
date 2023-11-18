using GearShop.Contracts;
using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

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
				        Quantity = product.Rest
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
    }
}
