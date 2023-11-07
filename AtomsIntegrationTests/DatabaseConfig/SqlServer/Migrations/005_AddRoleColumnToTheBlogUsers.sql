IF (dbo.MigrationExists(4) = 1 AND dbo.MigrationExists(5) = 0)
BEGIN
	ALTER TABLE TheBlogUsers ADD UserRole NVARCHAR(20) NOT NULL;
	EXEC dbo.MigrateToVersion 5, '005_AddRoleColumnToTheBlogUsers';
END;