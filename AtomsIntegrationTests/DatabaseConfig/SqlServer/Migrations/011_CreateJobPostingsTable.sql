IF (dbo.MigrationExists(10) = 1 AND dbo.MigrationExists(11) = 0)
BEGIN
	CREATE TABLE JobPostings(
		[PostingId] BIGINT NOT NULL,
		[EmployerId] BIGINT NOT NULL,
		[Description] NVARCHAR(1024) NULL
	);
	CREATE UNIQUE INDEX IX_JobPostings ON JobPostings (PostingId, EmployerId);
	EXEC dbo.MigrateToVersion 11, '011_CreateJobPostingsTable';
END;