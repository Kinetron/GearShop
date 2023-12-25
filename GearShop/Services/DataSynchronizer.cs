using DataParser;
using GearShop.Contracts;
using GearShop.Controllers;
using GearShop.Enums;
using GearShop.Models.Entities;
using GearShop.Services.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Serilog;
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

		/// <summary>
		/// Название источника в таблице InfoSource для синхронизации прайс листа.
		/// </summary>
		private const string PriceSourceName = "Из прайса";

		public DataSynchronizer(GearShopDbContext dbContext, ILogger<HomeController> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public string LastError { get; private set; }

		/// <summary>
		/// Синхронизирует данные в БД с файлом CSV.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public async Task<bool> CsvSynchronize(string fileName, string shopName)
		{
			CsvParser parser = new CsvParser();
			List<DataParser.Models.Product> products = parser.ParseFile(fileName, '|');
			if (products == null)
			{
				_logger.LogError("Empty model");
				return false;
			}


			int currentRow = 1;
			string operationName = "Синхронизация сведений о продуктах";
			int operationId = 1;

			DateTime beginDt = DateTime.Now;

			//Строка в таблице которая будет содержать статус синхронизации данных.
			PriceSynchronizeStatus synchronizeStatus = 
				await _dbContext.PriceSynchronizeStatus.FirstOrDefaultAsync(s=>s.OperationId == operationId);

			//Таблица пуста.
			if (synchronizeStatus == null)
			{
				synchronizeStatus = new PriceSynchronizeStatus();
				_dbContext.PriceSynchronizeStatus.Add(synchronizeStatus);
			}
			
			//Сброс на начальные значения.
			synchronizeStatus.Current = currentRow;
			synchronizeStatus.Total = products.Count();
			synchronizeStatus.CurrentOperation = operationName;
			synchronizeStatus.OperationId = operationId;
			synchronizeStatus.BeginOperation = beginDt;
			synchronizeStatus.ErrorText = string.Empty;


			int? shopId = (await _dbContext.Shops.FirstOrDefaultAsync(s => s.Name == shopName))?.Id;
			if (shopId == null)
			{
				_logger.LogError("Bad shop name");
				return false;
			}

			try
			{
				_dbContext.SaveChanges();
				
				//Идентификатор продукта с не известными типом.
				var defaultType = await _dbContext.ProductTypes.FirstAsync(x => x.Name == "Прочие");
				int defaultTypeId = defaultType.Id;

				//Идентификатор источника данных
				int? infoSourceId = _dbContext.InfoSource.FirstOrDefault(x => x.Name == PriceSourceName)?.Id;
				if (infoSourceId == null)
				{
					_logger.LogError($"Not found id for InfoSource with name {PriceSourceName}");
					return false;
				}

				//Правила синхронизации.
				var defaultSynchronizationRule = await _dbContext.SynchronizationRules
					.FirstOrDefaultAsync(x => x.Code == (int)SynchronizationRulesEnum.None);

				if (defaultSynchronizationRule == null)
				{
					_logger.LogError("Not found default id for SynchronizationRules");
					return false;
				}

				int defaultSynchronizationRuleId = defaultSynchronizationRule.Id;

				foreach (var item in products)
				{
					//Тип продукта.
					int? productTypeId = null;
					var currentType =
						await _dbContext.ProductTypes.FirstOrDefaultAsync(x => x.Name == item.ProductTypeName);

					if (currentType != null)
					{
						productTypeId = currentType.Id;
					}

					if (!productTypeId.HasValue) productTypeId = defaultTypeId;

					//Существует ли продукт?
					if (await _dbContext.Products.CountAsync(x => x.Name == item.Name) > 0)
					{
						Product exists = await _dbContext.Products.FirstAsync(x => x.Name == item.Name);
						exists.Deleted = 0;
						exists.PurchaseCost = item.PurchaseCost;
						exists.RetailCost = item.RetailCost;
						exists.WholesaleCost = item.WholesaleCost;
						exists.Rest = item.Rest;
						exists.Available = item.Available;
						exists.Changed = DateTime.Now;

						//Картинки нет. Добавим.
						if (string.IsNullOrEmpty(exists.ImageName))
						{
							exists.ImageName = item.ImageName;
						}

						//Тип синхронизации продукта. Переделать на include
						var synchronizationRule = await 
							_dbContext.SynchronizationRules.FirstAsync(x=>x.Id == exists.SynchronizationRuleId);

						//Игнорирование типа и картинки с прайса.
						if (synchronizationRule.Code != (int)SynchronizationRulesEnum.IgnoredTypeAndImageFromPrice)
						{
							exists.ProductTypeId = productTypeId.Value;
							exists.ImageName = item.ImageName;
						}

					}
					else
					{
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
							InfoSourceId = infoSourceId.Value,
							SynchronizationRuleId = defaultSynchronizationRuleId,
							ShopId = shopId.Value,
						};
						
						product.Created = DateTime.Now;
						product.Changed = product.Created;
						await _dbContext.Products.AddAsync(product);
					}

					currentRow++;
					if (currentRow % StepProgress == 0)
					{
						synchronizeStatus.Current = currentRow;
					}

					await _dbContext.SaveChangesAsync();
				}

				//Все продукты которых нет в прайс листе будут иметь дату раньше.
				// Удалим продукты которых нет в прайсе.
				await DeleteEarlyProducts(beginDt, infoSourceId.Value); //Обновить все продукты 

				//Операция завершена.
				synchronizeStatus.Current = synchronizeStatus.Total;
				synchronizeStatus.EndOperation = DateTime.Now;
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				synchronizeStatus.ErrorText = ex.Message;
				await _dbContext.SaveChangesAsync();

				_logger.LogError($"Ошибка обновления строк", ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Удаляет продукты которых нет в прайсе.
		/// </summary>
		/// <param name="beginDt"></param>
		/// <param name="infoSourceId"></param>
		private async Task<bool> DeleteEarlyProducts(DateTime beginDt, int infoSourceId)
		{
			var products = await _dbContext.Products.Where(p => p.Changed < beginDt
			                                                    && p.InfoSourceId == infoSourceId).ToListAsync();
			products.ForEach(p=>
			{
				p.Deleted = 1;
				p.Changed = DateTime.Now;
			});

			await _dbContext.SaveChangesAsync();

			return true;
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

			try
			{
				CleanDir(imageDir);
				Archivator.UnpackSplitZip(zipFile, imageDir);
				List<string> files = Directory.GetFiles(imageDir, "*.*", SearchOption.AllDirectories).ToList();
				
				foreach (string file in files)
				{
					FileInfo mFile = new FileInfo(file);
					mFile.MoveTo(Path.Combine(@"wwwroot", "productImages", mFile.Name), true);
				}
			}
			catch (Exception ex)
			{
				LastError = $"Исключение {ex.Message} {ex.StackTrace}";
				return false;
			}

			return true;
		}

		/// <summary>
		/// Возвращает информацию о текущей выполняемой операции(например синхронизация данных).
		/// </summary>
		/// <param name="operationId"></param>
		/// <returns></returns>
		public async Task<string> GetOperationStatus(int operationId)
		{
			try
			{
				var info =
					await _dbContext.PriceSynchronizeStatus.FirstOrDefaultAsync(o => o.OperationId == operationId);

				if (info == null) return null;
				return JsonConvert.SerializeObject(info, Formatting.Indented);
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex);
				return null;
			}
		}

		/// <summary>
		/// Удаляет все файлы в директории.
		/// </summary>
		/// <param name="dir"></param>
		private void CleanDir(string dir)
		{
			if (!Directory.Exists(dir)) return;

			foreach (string file in Directory.GetFiles(dir))
			{
				FileInfo fi = new FileInfo(file);
				fi.Delete();
			}
		}
	}
}
