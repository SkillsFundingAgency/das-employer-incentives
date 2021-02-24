CREATE TABLE [archive].[Payment]
(
    [PaymentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
    [ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
    [PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] BIGINT NOT NULL,
    [AccountLegalEntityId] BIGINT NOT NULL,
    [CalculatedDate] DATETIME2 NOT NULL,
    [PaidDate] DATETIME2 NULL,
    [SubNominalCode] INT NOT NULL,
    [PaymentPeriod] TINYINT NOT NULL,
    [PaymentYear] SMALLINT NOT NULL,
    [Amount] DECIMAL(9, 2) NOT NULL,
    [ArchivedDateUTC] DATETIME2 NOT NULL
)
GO