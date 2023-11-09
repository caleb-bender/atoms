IF (dbo.MigrationExists(6) = 1 AND dbo.MigrationExists(7) = 0)
BEGIN
	CREATE TABLE BlogPosts(
		PostId BIGINT NOT NULL,
		Genre NVARCHAR(20) NOT NULL,
		Title NVARCHAR(255) NOT NULL,
		Content NVARCHAR(MAX) NOT NULL, 
		CONSTRAINT PK_BlogPosts PRIMARY KEY (PostId, Genre)
	);
	EXEC dbo.MigrateToVersion 7, '007_CreateBlogPostsTable';
END;