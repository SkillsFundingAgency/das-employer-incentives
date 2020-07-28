CREATE TABLE [dbo].[IncentiveApplicationApprenticeship]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[IncentiveApplicationId] UNIQUEIDENTIFIER NOT NULL,
	[ApprenticeshipId] INT NOT NULL,
	[FirstName] NVARCHAR(100) NOT NULL,
	[LastName] NVARCHAR(100) NOT NULL,
	[DateOfBirth] DATETIME NOT NULL,
	[ULN] INT NOT NULL,
	[PlannedStartDate] DATETIME NOT NULL,
	[ApprenticeshipEmployerTypeOnApproval] NVARCHAR(50),
	CONSTRAINT FK_IncentiveApplication FOREIGN KEY (IncentiveApplicationId) REFERENCES IncentiveApplication(Id)
)

