CREATE TABLE [archive].[PendingPayment]
(
    [PendingPaymentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
    [AccountId] BIGINT NOT NULL, 
    [ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
    [DueDate] DATETIME2 NOT NULL, 
    [Amount] DECIMAL(9, 2) NOT NULL, 
    [CalculatedDate] DATETIME2 NOT NULL, 
    [PaymentMadeDate] DATETIME2 NULL,
    [PeriodNumber] TINYINT NULL,
    [PaymentYear] SMALLINT NULL,
    [AccountLegalEntityId] BIGINT NULL,
    [EarningType] VARCHAR(20) NULL,
    [ClawedBack] BIT NOT NULL,
    [ArchivedDateUTC] DATETIME2 NOT NULL
)
GO