CREATE TABLE [dbo].[ApprenticeshipEarning]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [IncentiveClaimApprenticeshipId] UNIQUEIDENTIFIER NOT NULL, 
    [AccountId] BIGINT NOT NULL, 
    [DatePayable] DATETIME2 NOT NULL, 
    [AmountPayable] DECIMAL(9, 2) NOT NULL, 
    [DateCalculated] DATETIME2 NOT NULL, 
    [DatePaymentCalculated] DATETIME2 NULL
)

GO
CREATE INDEX IX_ApprenticeshipEarning_IncentiveClaimApprenticeshipId ON ApprenticeshipEarning (IncentiveClaimApprenticeshipId)
GO
CREATE INDEX IX_ApprenticeshipEarning_AccountId ON ApprenticeshipEarning (AccountId)
GO
