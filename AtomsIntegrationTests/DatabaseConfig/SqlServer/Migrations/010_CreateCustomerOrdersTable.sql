IF (dbo.MigrationExists(9) = 1 AND dbo.MigrationExists(10) = 0)
BEGIN
	CREATE TABLE CustomerOrders(
		OrderId UNIQUEIDENTIFIER PRIMARY KEY,
		OrderType VARCHAR(10) NULL,
	)
	EXEC dbo.MigrateToVersion 10, '010_CreateCustomerOrdersTable';
END;