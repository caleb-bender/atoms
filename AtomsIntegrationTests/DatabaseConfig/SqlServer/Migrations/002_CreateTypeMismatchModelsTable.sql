IF (dbo.MigrationExists(1) = 1 AND dbo.MigrationExists(2) = 0)
BEGIN
	CREATE TABLE TypeMismatchModels(
		[Id] UNIQUEIDENTIFIER PRIMARY KEY,
		[Status] NVARCHAR(100) NOT NULL
	);
	EXEC dbo.MigrateToVersion 2, '002_CreateTypeMismatchModelsTable';
END;