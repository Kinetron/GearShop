using System.Text;
using DataParser.Models;
using GearShop.Models.Dto.Products;
using GearShop.Models.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GearShop.Services
{
	/// <summary>
	/// Конвертер списка продуктов в заказе в html.
	/// </summary>
	public class OrderToHtmlConverter
	{
		private const string tableBiginTag = "<table>\r\n";
		private const string tableEndTag = "</table>\r\n";
		private const string rowBiginTag = "<tr>\r\n";
		private const string rowEndTag = "</tr>\r\n";
		private const string columnBiginTag = "<td>\r\n";
		private const string columnEndTag = "</td>\r\n";
		

		public string Convert(List<ProductDto> products, OrderInfo orderInfo, long orderId)
		{
			decimal totalAmount = products.Sum(x => x.Amount * x.Cost);
			StringBuilder sb = new StringBuilder();
			sb.Append(ParagraphTag($"Создан заказ № {orderId} на сумму {totalAmount} рублей"));
			sb.Append(ParagraphTag($"от {orderInfo.BuyerName} {orderInfo.BuyerPhone} {orderInfo.BuyerEmail}"));
			sb.Append(ParagraphTag(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")));

			string[] tableTitles = { "Товар", "Количество", "Сумма 1шт"};

			sb.Append(tableBiginTag);
			sb.Append(CreateHtmlTableRow(tableTitles));

			foreach (var product in products)
			{
				string[] row = { product.Name, product.Amount.ToString(), product.Cost.ToString()};
				sb.Append(CreateHtmlTableRow(row));
			}

			sb.Append(tableEndTag);

			return sb.ToString();
		}

		/// <summary>
		/// Создает строку таблицы.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private string CreateHtmlTableRow(string[] data)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(rowBiginTag);
			foreach (var column in data)
			{
				sb.Append(columnBiginTag);
				sb.Append(column);
				sb.Append(columnEndTag);
			}
			sb.Append(rowEndTag);

			return sb.ToString();
		}
		
		private string ParagraphTag(string innerText)
		{
			return $"<p>{innerText}</p>";
		}
	}
}
