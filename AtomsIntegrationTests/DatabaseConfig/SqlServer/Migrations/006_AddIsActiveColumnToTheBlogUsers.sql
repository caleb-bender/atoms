IF (dbo.MigrationExists(5) = 1 AND dbo.MigrationExists(6) = 0)
BEGIN
	ALTER TABLE TheBlogUsers ADD IsActive BIT DEFAULT 1 NOT NULL;
	EXEC dbo.MigrateToVersion 6, '006_AddIsActiveColumnToTheBlogUsers';
END;