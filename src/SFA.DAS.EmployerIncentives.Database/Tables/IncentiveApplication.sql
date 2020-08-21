﻿CREATE TABLE [dbo].[IncentiveApplication]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[AccountId] BIGINT NOT NULL,
	[AccountLegalEntityId] BIGINT NOT NULL,
	[DateCreated] DATETIME NOT NULL,
	[Status] NVARCHAR(50) NOT NULL,
	[DateSubmitted] DATETIME2 NULL,
	[SubmittedByEmail] NVARCHAR(50) NULL,
	[SubmittedByName] NVARCHAR(100) NULL
)
GO
CREATE INDEX IX_IncentiveApplication_AccountId ON IncentiveApplication (AccountId)
GO