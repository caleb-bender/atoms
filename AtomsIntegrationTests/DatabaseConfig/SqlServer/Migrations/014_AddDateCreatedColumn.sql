IF (dbo.MigrationExists(13) = 1 AND dbo.MigrationExists(14) = 0)
BEGIN
	ALTER TABLE TypeMismatchModels ADD DateCreated DATE NULL;
	EXEC dbo.MigrateToVersion 14, '014_AddDateCreatedColumn';
END;