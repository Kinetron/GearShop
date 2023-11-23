using DataParser;
using GearShop.Contracts;
using GearShop.Controllers;
using GearShop.Models.Entities;
using GearShop.Services.Repository;
using Microsoft.EntityFrameworkCore.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GearShop.Services
{
    /// <summary>
    /// Синхронизирует данные БД с внешними данными(например с файлом csv).
    /// </summary>
    public class DataSynchronizer : IDataSynchronizer
	{
		/// <summary>
		/// Шаг через который будет добавлена информация о процессе синхронизации.
		/// </summary>
		private const int StepProgress = 100;
		private readonly GearShopDbContext _dbContext;
		private readonly ILogger<HomeController> _logger;
		
		public DataSynchronizer(GearShopDbContext dbContext, ILogger<HomeController> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		/// <summary>
		/// Синхронизирует данные в БД с файлом CSV.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public bool CsvSynchronize(string fileName)
		{
			CsvParser parser = new CsvParser();
			List<DataParser.Models.Product> products = parser.ParseFile(fileName, '|');
			if (products == null)
			{
				_logger.LogError("Empty model");
				return false;
			}


			int currentRow = 1;

			var info = new PriceSynchronizeStatus()
			{
				Current = currentRow,
				Total = products.Count()
			};

			//Информация для прогресс бара.
			//_dbContext.PriceSynchronizeStatus.Add(info);

			//_dbContext.SaveChanges();

			DateTime beginDt = DateTime.Now;
			try
			{
				foreach (var item in products)
				{
					item.Name = FilteredName(item.Name);

					Product product = new Product()
					{
						Name = item.Name,
						RetailCost = item.RetailCost,
						PurchaseCost = item.PurchaseCost,
						WholesaleCost = item.WholesaleCost,
						Rest = item.Rest,
						ImageName = item.ImageName,
						Available = item.Available
					};
					
					//Существует ли продукт?
					if (_dbContext.Products.Count(x => x.Name == item.Name) > 0)
					{
						Product exists = _dbContext.Products.First(x => x.Name == item.Name);
						exists.Deleted = 0;
						exists.PurchaseCost = item.PurchaseCost;
						exists.RetailCost = item.RetailCost;
						exists.WholesaleCost = item.WholesaleCost;
						exists.Rest = item.Rest;
						exists.Available = item.Available;
						exists.ImageName = item.ImageName;
						exists.Changed = DateTime.Now;
					}
					else
					{
						product.Created = DateTime.Now;
						product.Changed = product.Created;
						_dbContext.Products.Add(product);
					}

					//currentRow++;
					//if (currentRow % StepProgress == 0)
					//{
					//	var progress = _dbContext.PriceSynchronizeStatus.First();
					//	progress.Current = currentRow;
					//}

					_dbContext.SaveChanges();
				}

				//Все продукты которых нет в прайс листе будут иметь дату раньше.
				// Удалим продукты которых нет в прайсе.
				DeleteEarlyProducts(beginDt); //Обновить все продукты 
			}
			catch (Exception ex)
			{
				_logger.LogError($"Ошибка обновления строк", ex);
			}

			return true;
		}

		/// <summary>
		/// Удаляет продукты которых нет в прайсе.
		/// </summary>
		/// <param name="beginDt"></param>
		private void DeleteEarlyProducts(DateTime beginDt)
		{
			var products = _dbContext.Products.Where(p => p.Changed < beginDt).ToList();
			products.ForEach(p=>
			{
				p.Deleted = 1;
				p.Changed = DateTime.Now;
			});

			_dbContext.SaveChanges();
		}

		/// <summary>
		/// Вынести в прогу загрузчик!
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private string FilteredName(string data)
		{
			string[] filter = data.Split(',');
			return filter[0];
		}
	}
}
