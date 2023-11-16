//namespace GearShop.Contracts;

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
    }
}