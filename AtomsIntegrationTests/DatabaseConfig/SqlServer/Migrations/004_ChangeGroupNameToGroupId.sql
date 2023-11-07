IF (dbo.MigrationExists(3) = 1 AND dbo.MigrationExists(4) = 0)
BEGIN
	ALTER TABLE TheBlogUsers DROP CONSTRAINT PK_TheBlogUsers;
	ALTER TABLE TheBlogUsers DROP COLUMN GroupName;
	ALTER TABLE TheBlogUsers ADD GroupId NVARCHAR(50) NOT NULL;
	ALTER TABLE TheBlogUsers ADD CONSTRAINT PK_TheBlogUsers PRIMARY KEY (UserId, GroupId);
	EXEC dbo.MigrateToVersion 4, '004_ChangeGroupNameToGroupId';
END;