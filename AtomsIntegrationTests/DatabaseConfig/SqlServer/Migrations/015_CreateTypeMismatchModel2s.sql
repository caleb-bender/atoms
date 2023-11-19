IF (dbo.MigrationExists(14) = 1 AND dbo.MigrationExists(15) = 0)
BEGIN
	CREATE TABLE TypeMismatchModel2s(
		TheId BIGINT PRIMARY KEY,
		DateCreated DATE NOT NULL
	);
	EXEC dbo.MigrateToVersion 15, '015_CreateTypeMismatchModel2s';
END;