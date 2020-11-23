CREATE TABLE [dbo].[IncentiveApplicationApprenticeship]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[IncentiveApplicationId] UNIQUEIDENTIFIER NOT NULL,
	[ApprenticeshipId] BIGINT NOT NULL,
	[FirstName] NVARCHAR(100) NOT NULL,
	[LastName] NVARCHAR(100) NOT NULL,
	[DateOfBirth] DATETIME2 NOT NULL,
	[ULN] BIGINT NOT NULL,
	[PlannedStartDate] DATETIME2 NOT NULL,
	[ApprenticeshipEmployerTypeOnApproval] INT NOT NULL,
	[TotalIncentiveAmount] MONEY NOT NULL, 
    [UKPRN] BIGINT NULL, 
	[EarningsCalculated] [bit] NOT NULL  DEFAULT 0,	
    CONSTRAINT FK_IncentiveApplication FOREIGN KEY (IncentiveApplicationId) REFERENCES IncentiveApplication(Id)
)
GO
CREATE INDEX IX_IncentiveApplicationApprenticeship_ApplicationId ON IncentiveApplicationApprenticeship (IncentiveApplicationId)
GO
CREATE INDEX IX_IncentiveApplicationApprenticeship_Uln ON IncentiveApplicationApprenticeship ([ULN])
GO