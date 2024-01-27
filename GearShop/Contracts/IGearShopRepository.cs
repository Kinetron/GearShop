//namespace GearShop.Contracts;

using GearShop.Models;
using GearShop.Models.Dto;
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
		Task<List<ProductDto>> GetProducts(int currentPage, int itemsPerPage, string searchText, int productTypeId, bool available);

		/// <summary>
		/// Возвращает количество продуктов.
		/// </summary>
		/// <returns></returns>
		Task<int> GetProductCount(string searchText, int productTypeId, bool available);

		/// <summary>
		/// Получает список всех продуктов на складе.
		/// </summary>
		/// <returns></returns>
		Task<List<ProductDto>> GetProductsFromStockroom(int currentPage, int itemsPerPage, string searchText, int productTypeId, bool available);
		
		/// <summary>
		/// Проверяет наличие guid в БД. Если нет – добавляет новую запись.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		Task<bool> SynchronizeNoRegUserGuidAsync(string guid, string ipAddress);

		/// <summary>
		/// Создает заказ. Возвращает номер заказа.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		Task<long> CreateOrder(List<ProductDto> model, OrderInfo orderInfo, string userGuid, string ip);

		/// <summary>
		/// Заказы пользователя.
		/// </summary>
		/// <param name="noRegUseGuid"></param>
		/// <returns></returns>
		Task<List<OrderDto>> GetOrderList(string noRegUseGuid);

        /// <summary>
        /// Данные для отображения слайдера на главной странице.
        /// </summary>
        /// <returns></returns>
		Task<List<SlaiderMainPageDto>> MainPageSlaiderDataAsync();

        /// <summary>
        /// Возвращает список продуктов.
        /// </summary>
        /// <returns></returns>
        Task<List<ProductType>> GetProductTypesAsync();

		/// <summary>
		/// Возвращает название картинок и название продукта к которому относиться картинка.
		/// </summary>
		/// <returns></returns>
		Task<List<KeyValuePair<string, string>>> GetProductImagesInfoAsync();

        /// <summary>
        /// Добавляет продукт в прайс.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
		Task<bool> CreateProductAsync(ProductDto model);

        /// <summary>
        /// Обновляет информацию о продукте в прайсе.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateProductAsync(ProductDto model);

        /// <summary>
        /// Удаляет продукт в прайсе.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> DeleteProductAsync(int id);
	
		Task<ArticleDto> GetPageContent(string pageName);
		Task<bool> UpdatePageContent(int id, string text);

		/// <summary>
		/// Возвращает список глав для родительской страницы.
		/// </summary>
		/// <param name="parentPageName"></param>
		/// <returns></returns>
		Task<List<ArticleDto>> GetArticleList(int pageId);

		/// <summary>
		/// Возвращает статью
		/// </summary>
		/// <returns></returns>
		Task<ArticleDto> GetArticle(int id);
		
		/// <summary>
		/// Return newsfeed.
		/// </summary>
		/// <returns></returns>
		Task<List<ArticleDto>> GetNewsfeed(int pageId);

		/// <summary>
		/// Добавляет статью.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="pageName"></param>
		/// <returns></returns>
		Task<bool> AddArticle(ArticleDto dto);


		Task<bool> UpdateArticle(ArticleDto dto);

		/// <summary>
		/// Удаляет статью.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteArticle(int id);

		/// <summary>
		/// Create database backup.
		/// </summary>
		/// <returns></returns>
		Task<string> BackupDbAsync();
    }
}