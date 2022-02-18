CREATE TABLE [dbo].[Accounts]
(
    [Id] BIGINT NOT NULL,
    [AccountLegalEntityId] BIGINT  NOT NULL,
    [LegalEntityId] BIGINT  NOT NULL,
    [HashedLegalEntityId] NVARCHAR(6) NULL, 
    [LegalEntityName] VARCHAR(MAX)  NOT NULL,
    [SignedAgreementVersion] INT NULL,
    [VrfVendorId] NVARCHAR(100) NULL, 
    [VrfCaseId] NVARCHAR(100) NULL, 
    [VrfCaseStatus] NVARCHAR(100) NULL, 
    [VrfCaseStatusLastUpdatedDateTime] DATETIME2 NULL, 
    [VendorBlockEndDate] DATETIME2 NULL
    CONSTRAINT PK_Accounts PRIMARY KEY NONCLUSTERED ([Id], [AccountLegalEntityId])
)
GO
CREATE INDEX IX_Account_AccountLegalEntityId ON Accounts (AccountLegalEntityId)
GO
CREATE INDEX IX_Account_VrfCaseStatus ON Accounts (VrfCaseStatus)
GO
CREATE INDEX IX_Account_LegalEntityId ON Accounts (LegalEntityId)
GO
CREATE INDEX IX_Account_HashedLegalEntityId ON Accounts (HashedLegalEntityId)
GO
CREATE INDEX IX_Account_VrfCaseStatusLastUpdatedDateTime ON Accounts (VrfCaseStatusLastUpdatedDateTime)
GO