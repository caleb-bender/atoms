IF (dbo.MigrationExists(15) = 1 AND dbo.MigrationExists(16) = 0)
BEGIN
	ALTER TABLE ModelsWithIgnored ADD PropertyWrittenAtCreationAndReadOnlyThereafter VARCHAR(50) NULL;
	EXEC dbo.MigrateToVersion 16, '016_AddColumnToModelsWithIgnoredTable';
END;