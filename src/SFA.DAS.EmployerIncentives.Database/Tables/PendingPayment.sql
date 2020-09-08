CREATE TABLE [dbo].[PendingPayment]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,	
    [AccountId] BIGINT NOT NULL, 
    [ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
    [DatePayable] DATETIME2 NOT NULL, 
    [AmountPayablePence] INT NOT NULL, 
    [DateCalculated] DATETIME2 NOT NULL, 
    [DatePaymentMade] DATETIME2 NULL,
    CONSTRAINT FK_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES ApprenticeshipIncentive(Id)
)
GO
CREATE CLUSTERED INDEX IX_PendingPayment ON PendingPayment (AccountId)
GO

