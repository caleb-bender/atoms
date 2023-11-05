IF (dbo.MigrationExists(1) = 0)
BEGIN
	CREATE TABLE BlogPostAuthors(
		AuthorId BIGINT PRIMARY KEY,
		AuthorName NVARCHAR(100) NOT NULL,
		UserRegisteredOn DATE NOT NULL
	);
	EXEC dbo.MigrateToVersion 1, '001_CreateBlogPostAuthorsTable';
END;