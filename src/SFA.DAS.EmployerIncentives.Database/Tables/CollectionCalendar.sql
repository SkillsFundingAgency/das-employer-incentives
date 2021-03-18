CREATE TABLE [incentives].[CollectionCalendar]
(
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED,
	[PeriodNumber] TINYINT NOT NULL,	
	[CalendarMonth] TINYINT NOT NULL,
	[CalendarYear] SMALLINT NOT NULL,
	[EIScheduledOpenDateUTC] DATETIME2 NOT NULL, 
    [CensusDate] DATETIME NULL, 
    [AcademicYear] VARCHAR(10) NULL, 
    [Active] BIT NULL,
	[PeriodEndInProgress] BIT NOT NULL DEFAULT(0)
)
GO
CREATE CLUSTERED INDEX IX_CollectionCalendar_PeriodNumber ON [incentives].CollectionCalendar (PeriodNumber)
GO