IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'MigrateToVersion')
BEGIN
	EXEC('
	CREATE PROCEDURE MigrateToVersion
		@id INT,
		@migrationName VARCHAR(250)
	AS
	BEGIN
		INSERT INTO MigrationVersions(Id, MigrationName, ExecutedDate) VALUES(@id, @migrationName, GETDATE());
	END
	')
END;