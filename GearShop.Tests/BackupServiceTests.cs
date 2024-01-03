using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GearShop.Contracts;
using GearShop.Services;

namespace GearShop.Tests
{
	public class BackupServiceTests
	{
		[Test]
		public void ArchivingRootFiles()
		{
			IBackupService backupService = new BackupService();
			string path = backupService.ArchivingRootFiles().Result;
			Assert.IsNotNull(path);
		}
	}
}
