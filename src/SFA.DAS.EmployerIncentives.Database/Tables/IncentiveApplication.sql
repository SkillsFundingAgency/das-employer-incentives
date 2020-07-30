CREATE TABLE [dbo].[IncentiveApplication]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[AccountId] BIGINT NOT NULL,
	[AccountLegalEntityId] BIGINT NOT NULL,
	[DateCreated] DATETIME NOT NULL,
	[Status] NVARCHAR(50) NOT NULL,
	[DateSubmitted] DATETIME NULL,
	[SubmittedBy] NVARCHAR(50) NULL
)
