IF (dbo.MigrationExists(11) = 1 AND dbo.MigrationExists(12) = 0)
BEGIN
	CREATE TABLE Employees(
		EmployeeId UNIQUEIDENTIFIER NOT NULL,
		LocationId BIGINT NOT NULL IDENTITY,
		Salary DECIMAL(12,2) NOT NULL,
		CONSTRAINT PK_Employees PRIMARY KEY (EmployeeId, LocationId)
	);
	EXEC dbo.MigrateToVersion 12, '012_CreateEmployeesTable';
END;