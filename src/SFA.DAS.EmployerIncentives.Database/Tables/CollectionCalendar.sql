CREATE TABLE [incentives].[CollectionCalendar]
(
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED,
	[PeriodNumber] TINYINT NOT NULL,	
	[CalendarMonth] TINYINT NOT NULL,
	[CalendarYear] SMALLINT NOT NULL,
	[EIScheduledOpenDateUTC] DATETIME2 NOT NULL
)
GO
CREATE CLUSTERED INDEX IX_CollectionCalendar_PeriodNumber ON [incentives].CollectionCalendar (PeriodNumber)
GO