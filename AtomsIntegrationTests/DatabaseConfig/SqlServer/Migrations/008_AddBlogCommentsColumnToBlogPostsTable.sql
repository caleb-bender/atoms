IF (dbo.MigrationExists(7) = 1 AND dbo.MigrationExists(8) = 0)
BEGIN
	ALTER TABLE BlogPosts ADD BlogComments NVARCHAR(MAX) NULL;
	EXEC dbo.MigrateToVersion 8, '008_AddBlogCommentsColumnToBlogPostsTable';
END;