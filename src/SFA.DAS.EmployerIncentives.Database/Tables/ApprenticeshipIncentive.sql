﻿CREATE TABLE [incentives].ApprenticeshipIncentive
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[AccountId] BIGINT NOT NULL,	
	[ApprenticeshipId] BIGINT NOT NULL,
	[FirstName] NVARCHAR(100) NOT NULL,
	[LastName] NVARCHAR(100) NOT NULL,
	[DateOfBirth] DATETIME2 NOT NULL,
	[ULN] BIGINT NOT NULL,	
	[EmployerType] INT NOT NULL,
	[StartDate] DATETIME2 NOT NULL,
	[IncentiveApplicationApprenticeshipId] UNIQUEIDENTIFIER NOT NULL,
	[AccountLegalEntityId] BIGINT NULL, 
    [UKPRN] BIGINT NULL, 
	[RefreshedLearnerForEarnings] BIT NOT NULL DEFAULT(0), 
    [HasPossibleChangeOfCircumstances] BIT NOT NULL DEFAULT (0),    
    [PausePayments] BIT NOT NULL DEFAULT 0
)
GO
CREATE UNIQUE CLUSTERED INDEX IX_ApprenticeshipIncentive ON [incentives].[ApprenticeshipIncentive] (AccountId, ApprenticeshipId)
GO
