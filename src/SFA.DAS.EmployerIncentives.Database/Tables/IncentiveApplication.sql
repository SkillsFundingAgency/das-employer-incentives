CREATE TABLE [dbo].[IncentiveApplication]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[AccountId] INT NOT NULL,
	[AccountLegalEntityId] INT NOT NULL,
	[DateCreated] DATETIME NOT NULL,
	[Status] NVARCHAR(50) NOT NULL,
	[DateSubmitted] DATETIME2 NULL,
	[SubmittedBy] NVARCHAR(50) NULL
)
