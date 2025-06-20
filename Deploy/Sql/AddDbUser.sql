IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'TestGear')
BEGIN
    CREATE LOGIN TestGear WITH PASSWORD = '12345678', DEFAULT_DATABASE = GearShop;
END

-- Creating a user in the database
USE GearShop;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'TestGear')
BEGIN
    CREATE USER TestGear FOR LOGIN TestGear;
END

-- Granting the right to connect
GRANT CONNECT TO TestGear;
GRANT EXECUTE TO TestGear;

-- Assigning a role (for example, to read data)
EXEC sp_addrolemember 'db_datareader', 'TestGear';


-- If not connect
SELECT SERVERPROPERTY('IsIntegratedSecurityOnly');
-- If = 1 open SSMS and change mode in "Security"

