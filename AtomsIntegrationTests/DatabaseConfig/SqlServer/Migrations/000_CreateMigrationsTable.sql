IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MigrationVersions')
BEGIN
	CREATE TABLE MigrationVersions(
		Id INT PRIMARY KEY,
		MigrationName VARCHAR(250),
		ExecutedDate DATETIME
	);
END;