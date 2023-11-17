using DataParser;
using GearShop.Contracts;
using GearShop.Models.Entities;
using GearShop.Services.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace GearShop.Services
{
    /// <summary>
    /// Синхронизирует данные БД с внешними данными(например с файлом csv).
    /// </summary>
    public class DataSynchronizer : IDataSynchronizer
	{
		private readonly GearShopDbContext _dbContext;

		public DataSynchronizer(GearShopDbContext dbContext)
		{
			_dbContext = dbContext;
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
				//_sendErrorToUser($"{parser.LastError}{Environment.NewLine}");
				return false;
			}

			//using (var dbContextTransaction = _dbContext.BeginTransaction())
			//{
				foreach (var item in products)
				{
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

					_dbContext.Products.Add(product);
					try
					{
						_dbContext.SaveChanges();
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}
					
				}
				
			//	dbContextTransaction.Commit();
			//}

			return true;
		}
	}
}
