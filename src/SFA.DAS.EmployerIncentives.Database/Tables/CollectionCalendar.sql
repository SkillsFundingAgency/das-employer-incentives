CREATE TABLE [incentives].[CollectionCalendar]
(
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED,
	[PeriodNumber] SMALLINT NOT NULL,	
	[CalendarMonth] TINYINT NOT NULL,
	[CalendarYear] SMALLINT NOT NULL,
	[EIScheduledOpenDateUTC] DATETIME2 NOT NULL,
	[EIPeriodEndDateUTC] DATETIME2 NULL,
	[BCPaymentDateUTC] DATETIME2 NULL
)
GO
CREATE CLUSTERED INDEX IX_CollectionCalendar_PeriodNumber ON [incentives].CollectionCalendar (PeriodNumber)
GO