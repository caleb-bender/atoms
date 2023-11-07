IF (dbo.MigrationExists(2) = 1 AND dbo.MigrationExists(3) = 0)
BEGIN
	CREATE TABLE TheBlogUsers(
		UserId BIGINT,
		GroupName NVARCHAR(50),
		CONSTRAINT PK_TheBlogUsers PRIMARY KEY(UserId, GroupName)
	);
	EXEC dbo.MigrateToVersion 3, '003_CreateTheBlogUsersTable';
END;