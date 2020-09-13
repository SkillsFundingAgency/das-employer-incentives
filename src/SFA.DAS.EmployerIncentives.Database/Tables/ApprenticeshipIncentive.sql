CREATE TABLE [dbo].ApprenticeshipIncentive
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[AccountId] BIGINT NOT NULL,	
	[ApprenticeshipId] BIGINT NOT NULL,
	[FirstName] NVARCHAR(100) NOT NULL,
	[LastName] NVARCHAR(100) NOT NULL,
	[DateOfBirth] DATETIME2 NOT NULL,
	[ULN] BIGINT NOT NULL,	
	[EmployerType] INT NOT NULL,
	[PlannedStartDate] DATETIME2 NOT NULL,
	[IncentiveApplicationApprenticeshipId] UNIQUEIDENTIFIER NOT NULL
)
GO
CREATE UNIQUE CLUSTERED INDEX IX_ApprenticeshipIncentive ON ApprenticeshipIncentive (AccountId, ApprenticeshipId)
GO
