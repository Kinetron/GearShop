//namespace GearShop.Contracts;

using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;

namespace GearShop.Contracts
{
    public interface IGearShopRepository
    {
        /// <summary>
        /// Возвращает для пользователя хешированный пароль с солью. Если пользователя нет = null.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        string GetUserHashSalt(string userName);

        /// <summary>
        /// Возвращает код группы, используемый для разграничения прав.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        string GetUserGroupCode(string userName);

        /// <summary>
        /// Получить список всех продуктов.
        /// </summary>
        /// <returns></returns>
        public List<ProductDto> GetProducts(int currentPage, int itemsPerPage, string searchText);

        /// <summary>
        /// Возвращает количество продуктов.
        /// </summary>
        /// <returns></returns>
        int GetProductCount(string searchText);

		/// <summary>
		/// Проверяет наличие guid в БД. Если нет – добавляет новую запись.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		Task<bool> SynchronizeNoRegUserGuidAsync(string guid, string ipAddress);
    }
}