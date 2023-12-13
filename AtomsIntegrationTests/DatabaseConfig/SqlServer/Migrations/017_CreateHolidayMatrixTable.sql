IF (dbo.MigrationExists(16) = 1 AND dbo.MigrationExists(17) = 0)
BEGIN
	CREATE TABLE HolidayMatrices
	(
		HolidayDay VARCHAR(10) NOT NULL,
		RouteDay VARCHAR(10) NOT NULL,
		DaysToSkip INT NOT NULL,
		CONSTRAINT PK_HolidayMatrices PRIMARY KEY (HolidayDay, RouteDay)
	);
	EXEC dbo.MigrateToVersion 17, '017_CreateHolidayMatrixTable';
END;