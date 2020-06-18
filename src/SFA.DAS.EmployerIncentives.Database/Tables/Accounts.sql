CREATE TABLE [dbo].[Accounts]
(
	[Id] BIGINT NOT NULL PRIMARY KEY,
	[AccountLegalEntityId] BIGINT  NOT NULL,
	[LegalEntityId] BIGINT  NOT NULL,
	[LegalEntityName] VARCHAR(MAX)  NOT NULL
)
GO;
