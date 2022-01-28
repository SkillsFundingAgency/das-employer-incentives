CREATE TABLE [incentives].[EmploymentCheck]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[CheckType] NVARCHAR(50) NOT NULL,
	[MinimumDate] DATETIME2 NOT NULL,
	[MaximumDate] DATETIME2 NOT NULL,
	[CorrelationId] UNIQUEIDENTIFIER NOT NULL,
	[Result] BIT NULL,
	[CreatedDateTime] DATETIME2 NOT NULL,
	[UpdatedDateTime] DATETIME2 NULL,
	[ResultDateTime] DATETIME2 NULL,
	CONSTRAINT FK_EC_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id), 
)
GO
CREATE CLUSTERED INDEX IX_EmploymentCheck ON [incentives].[EmploymentCheck] (ApprenticeshipIncentiveId)
GO
CREATE INDEX IX_EmploymentCheck_CorrelationId ON [incentives].[EmploymentCheck] (CorrelationId)

GO


CREATE INDEX [IX_EmploymentCheck_CheckType] ON [incentives].[EmploymentCheck] ([CheckType])
