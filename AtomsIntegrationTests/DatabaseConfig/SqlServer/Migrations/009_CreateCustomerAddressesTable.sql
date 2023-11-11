IF (dbo.MigrationExists(8) = 1 AND dbo.MigrationExists(9) = 0)
BEGIN
	CREATE TABLE CustomerAddresses(
		Phone VARCHAR(20) PRIMARY KEY,
		Unit INT NULL,
		StreetNumber INT NULL,
		Street NVARCHAR(100) NULL,
		PostalCode VARCHAR(20) NULL,
		City NVARCHAR(50) NULL,
		Province NVARCHAR(50) NULL,
		Country NVARCHAR(50) NULL
	)
	EXEC dbo.MigrateToVersion 9, '009_CreateCustomerAddressesTable';
END;