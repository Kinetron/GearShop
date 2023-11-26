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
	}
}
