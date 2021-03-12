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
    [EarningType] VARCHAR(20) NULL
    CONSTRAINT FK_PendingPaymentArchive_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id), 
    [ClawedBack] BIT NOT NULL DEFAULT 0,
	[ArchiveDateUTC] DATETIME2 NOT NULL,
)
GO
CREATE CLUSTERED INDEX IX_ArchivePendingPayment ON [archive].[PendingPayment] (AccountId)
GO
CREATE INDEX IX_ArchivePendingPayment_ApprenticeshipIncentiveId ON [archive].[PendingPayment] (ApprenticeshipIncentiveId)
GO 