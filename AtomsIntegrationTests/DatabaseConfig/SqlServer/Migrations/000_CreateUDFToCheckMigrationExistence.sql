IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'MigrationExists')
BEGIN
	EXEC('
	CREATE FUNCTION MigrationExists(@id INT)
	RETURNS BIT
	AS
	BEGIN
		DECLARE @doesExist AS BIT;
		IF EXISTS (SELECT 1 FROM MigrationVersions WHERE Id = @id)
			SET @doesExist = 1;
		ELSE
			SET @doesExist = 0;
		RETURN @doesExist
	END;
	')
END;