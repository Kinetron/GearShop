using GearShop.Contracts;
using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;

namespace GearShop.Services
{
    /// <summary>
    /// Сервис уведомлений.
    /// </summary>
    public class Notifier : INotifier
	{
		/// <summary>
		/// Почта на которую будет приходить информация о заказах.
		/// </summary>
		private readonly string _managerEmail;

		private readonly IEMailNotifier _eMailNotifier;
		private readonly ILogger<Notifier> _logger;

		public Notifier(string managerEmail, IEMailNotifier eMailNotifier, ILogger<Notifier> logger)
		{
			_managerEmail = managerEmail;
			_eMailNotifier = eMailNotifier;
			_logger = logger;
		}

		/// <summary>
		/// Новый заказ.
		/// </summary>
		/// <param name="products"></param>
		/// <param name="orderInfo"></param>
		/// <param name="orderId"></param>
		public void NewOrder(List<ProductDto> products, OrderInfo orderInfo, long orderId)
		{
			Task.Run(() =>
			{
				OrderToHtmlConverter converter = new OrderToHtmlConverter();
				string html = converter.Convert(products, orderInfo, orderId);

				bool result = _eMailNotifier.SendFromMailRu(_managerEmail, "Новый заказ", html); //Шлю письмо менеджеру
				if (!result)
				{
					_logger.LogError(_eMailNotifier.LastError);
				}

				//Добавить формата почты.
				if (!string.IsNullOrEmpty(orderInfo.BuyerEmail))
				{
					result = _eMailNotifier.SendFromMailRu(orderInfo.BuyerEmail, "Заказ", html); //Шлю письмо клиенту.
					if (!result)
					{
						_logger.LogError(_eMailNotifier.LastError);
					}
				}
			});
		}

		/// <summary>
		/// Отправляет сообщение менеджеру.
		/// </summary>
		/// <param name="senderName"></param>
		/// <param name="senderEmail"></param>
		/// <param name="senderText"></param>
		/// <returns></returns>
		public async Task<bool> SendMessageToManagerAsync(string senderName, string senderEmail, string senderText, string remoteIpAddress)
		{
			return await Task.Run(() =>
			{
				string body = $"<p>{senderName}</p><p>{senderEmail}</p><p>{remoteIpAddress}</p><p>{senderText}</p>";

				//Шлю письмо менеджеру
				bool result = _eMailNotifier.SendFromMailRu(_managerEmail, "Новое сообщение с сайта",
					body);

				if (!result)
				{
					_logger.LogError(_eMailNotifier.LastError);
				}

				return result;
			});
		}
	}
}
