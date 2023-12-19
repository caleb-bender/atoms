IF (dbo.MigrationExists(17) = 1 AND dbo.MigrationExists(18) = 0)
BEGIN
	CREATE TABLE OneIdentityPropertyModels(Id BIGINT IDENTITY PRIMARY KEY);
	EXEC dbo.MigrateToVersion 18, '018_CreateOneIdentityPropertyModelTable';
END;