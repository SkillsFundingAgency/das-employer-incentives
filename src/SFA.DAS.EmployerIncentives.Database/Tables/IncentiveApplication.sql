CREATE TABLE [dbo].[IncentiveApplication]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[AccountId] BIGINT NOT NULL,
	[AccountLegalEntityId] BIGINT NOT NULL,
	[DateCreated] DATETIME NOT NULL,
	[Status] NVARCHAR(50) NOT NULL,
	[DateSubmitted] DATETIME2 NULL,
	[SubmittedByEmail] NVARCHAR(255) NULL,
	[SubmittedByName] NVARCHAR(MAX) NULL
)
GO
CREATE INDEX IX_IncentiveApplication_AccountId ON IncentiveApplication (AccountId)
GO
CREATE INDEX IX_IncentiveApplication_Status ON IncentiveApplication ([Status]) INCLUDE (AccountLegalEntityId)
GO
CREATE INDEX IX_IncentiveApplication_AccountLegalEntityId ON IncentiveApplication (AccountLegalEntityId)
GO