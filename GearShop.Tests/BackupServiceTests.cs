using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GearShop.Contracts;
using GearShop.Services;
using Moq;

namespace GearShop.Tests
{
	public class BackupServiceTests
	{
		[Test]
		public void ArchivingRootFiles()
		{
			var repo = new Mock<IGearShopRepository>();
			IBackupService backupService = new BackupService(true, "", repo.Object);
			string path = backupService.ArchivingRootFiles().Result;
			Assert.IsNotNull(path);
		}

		[Test]
		public void CreateDbBackup()
		{
			var repo = new Mock<IGearShopRepository>();
			repo.Setup(x=>x.BackupDbAsync()).Returns(Task.FromResult("path Db"));
			IBackupService backupService = new BackupService(true, "g:\\tmp\\", repo.Object);
			string path = backupService.CreateDbBackup().Result;
			Assert.IsNotNull(path);
		}

		[Test]
		public void DenyCreateDbBackup()
		{
			var repo = new Mock<IGearShopRepository>();
			repo.Setup(x => x.BackupDbAsync()).Returns(Task.FromResult("path Db"));
			IBackupService backupService = new BackupService(false, "g:\\tmp\\", repo.Object);
			string path = backupService.CreateDbBackup().Result;
			Assert.IsNull(path);
		}
	}
}
