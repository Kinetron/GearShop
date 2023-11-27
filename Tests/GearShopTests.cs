using GearShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;

namespace Tests
{
	public class GearShopTests
	{
		/// <summary>
		/// Теструет конвертер списка продуктов в заказе в html.
		/// </summary>
		[Test]
		public void ConverterOrderToHtml()
		{
			List<ProductDto> products = new List<ProductDto>()
			{
				new ProductDto()
				{
					Name = "Сальник для Газель",
					Cost = 150.12m,
					Amount = 10
				},
				new ProductDto()
				{
					Name = "Клапан раздачи RTK894",
					Cost = 2500.03m,
					Amount = 3
				},
			};

			OrderInfo orderInfo = new OrderInfo()
			{
				BuyerName = "Петя",
				BuyerPhone = "+78945245",
				BuyerEmail = "pet@mail"
			};

			OrderToHtmlConverter converter = new OrderToHtmlConverter();
			string result = converter.Convert(products, orderInfo, 501);
			Assert.IsNotEmpty(result);
		}
	}
}
