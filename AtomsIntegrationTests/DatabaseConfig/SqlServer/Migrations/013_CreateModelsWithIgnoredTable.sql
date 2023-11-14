IF (dbo.MigrationExists(12) = 1 AND dbo.MigrationExists(13) = 0)
BEGIN
	CREATE TABLE ModelsWithIgnored(
		Id BIGINT PRIMARY KEY,
		PropertyReadFromButNotWrittenTo VARCHAR(50) NULL,
		PropertyNeitherReadFromNorWrittenTo VARCHAR(50) NULL
	);
	EXEC dbo.MigrateToVersion 13, '013_CreateModelsWithIgnoredTable';
END;