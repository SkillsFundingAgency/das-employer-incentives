CREATE TABLE [dbo].[Accounts]
(
	[Id] BIGINT NOT NULL,
	[AccountLegalEntityId] BIGINT  NOT NULL,
	[LegalEntityId] BIGINT  NOT NULL,
	[LegalEntityName] VARCHAR(MAX)  NOT NULL,
	[HasSignedIncentivesTerms] BIT NOT NULL DEFAULT(0),
	CONSTRAINT PK_Accounts PRIMARY KEY NONCLUSTERED ([Id], [AccountLegalEntityId])
)
GO
CREATE INDEX IX_Account_AccountLegalEntityId ON Accounts (AccountLegalEntityId)
GO