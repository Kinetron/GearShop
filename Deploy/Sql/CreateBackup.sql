BACKUP DATABASE [GearShop]
TO DISK = 'C:\Temp\DeployDb.bak'
WITH INIT, 
     NAME = 'GearShop-Full Backup',
     COMPRESSION,
     STATS = 10;
	

