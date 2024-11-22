IF (dbo.MigrationExists(18) = 1 AND dbo.MigrationExists(19) = 0)
BEGIN
	CREATE TABLE TimeDatas(Id INT IDENTITY, [Time] Time(7), [TimeSpan] Time(7));
	EXEC dbo.MigrateToVersion 19, '019_CreateTimeDataTable';
END;