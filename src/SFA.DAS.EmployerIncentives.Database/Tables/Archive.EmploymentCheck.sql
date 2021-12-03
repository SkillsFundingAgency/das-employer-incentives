CREATE TABLE [archive].[EmploymentCheck]
(
	[EmploymentCheckId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[CheckType] NVARCHAR(50) NOT NULL,
	[MinimumDate] DATETIME2 NOT NULL,
	[MaximumDate] DATETIME2 NOT NULL,
	[CorrelationId] UNIQUEIDENTIFIER NOT NULL,
	[Result] BIT NULL,
	[CreatedDateTime] DATETIME2 NOT NULL,
	[ResultDateTime] DATETIME2 NULL,
	[ArchiveDateUTC] DATETIME2 NOT NULL
)
GO
CREATE INDEX IX_ArchiveEmploymentCheck_ApprenticeshipIncentiveId ON [archive].[EmploymentCheck] (ApprenticeshipIncentiveId)
GO