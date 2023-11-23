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

		public Notifier(string managerEmail, IEMailNotifier eMailNotifier)
		{
			_managerEmail = managerEmail;
			_eMailNotifier = eMailNotifier;
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

				_eMailNotifier.SendFromMailRu(_managerEmail, "Новый заказ", html); //Шлю письмо менеджеру
				//Добавить формата почты.
				if (!string.IsNullOrEmpty(orderInfo.BuyerEmail))
				{
					_eMailNotifier.SendFromMailRu(orderInfo.BuyerEmail, "Заказ", html); //Шлю письмо клиенту.
				}
			});
		}
	}
}
