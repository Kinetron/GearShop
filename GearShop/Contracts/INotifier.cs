using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;

namespace GearShop.Contracts;

/// <summary>
/// Сервис уведомлений.
/// </summary>
public interface INotifier
{
    /// <summary>
    /// Новый заказ.
    /// </summary>
    /// <param name="products"></param>
    /// <param name="orderInfo"></param>
    /// <param name="orderId"></param>
    void NewOrder(List<ProductDto> products, OrderInfo orderInfo, long orderId);

    /// <summary>
    /// Отправляет сообщение менеджеру.
    /// </summary>
    /// <param name="senderName"></param>
    /// <param name="senderEmail"></param>
    /// <param name="senderText"></param>
    /// <returns></returns>
    public Task<bool> SendMessageToManagerAsync(string senderName, string senderEmail, string senderText, string remoteIpAddress);
}