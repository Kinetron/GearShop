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

			//Идентификатор продукта с не известными типом.
			int defaultTypeId = _dbContext.ProductTypes.First(x=>x.Name == "Прочие").Id;

			try
			{
				foreach (var item in products)
				{
					//Тип продукта.
					int? productTypeId =
						_dbContext.ProductTypes.FirstOrDefault(x => x.Name == item.ProductTypeName)?.Id;

					if (!productTypeId.HasValue) productTypeId = defaultTypeId;

					Product product = new Product()
					{
						Name = item.Name,
						RetailCost = item.RetailCost,
						PurchaseCost = item.PurchaseCost,
						WholesaleCost = item.WholesaleCost,
						Rest = item.Rest,
						ImageName = item.ImageName,
						Available = item.Available,
						ProductTypeId = productTypeId.Value,
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
						exists.ProductTypeId = productTypeId.Value;
						exists.Changed = DateTime.Now;
						
						//Картинки нет. Добавим.
						if (string.IsNullOrEmpty(exists.ImageName))
						{
							exists.ImageName =  item.ImageName;
						}
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
		/// Синхронизация картинок продуктов.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public bool ProductImagesSynchronize(string fileName, string storagePath)
		{
			string zipFile = Path.Combine(storagePath, fileName);
			string imageDir = Path.Combine(storagePath, Path.GetFileNameWithoutExtension(fileName));

			Archivator.UnpackSplitZip(zipFile, imageDir);


			List<string> files = Directory.GetFiles(imageDir, "*.*", SearchOption.AllDirectories).ToList();

			foreach (string file in files)
			{
				FileInfo mFile = new FileInfo(file);
				mFile.MoveTo(Path.Combine(@"wwwroot\productImages", mFile.Name));
			}

			return true;
		}
	}
}
