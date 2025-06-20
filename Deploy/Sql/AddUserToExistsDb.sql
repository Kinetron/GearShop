USE GearShop;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'TestGear')
BEGIN
    CREATE USER TestGear FOR LOGIN TestGear;
END

GRANT CONNECT TO TestGear;

EXEC sp_addrolemember 'db_datareader', 'TestGear'; 
EXEC sp_addrolemember 'db_datawriter', 'TestGear';