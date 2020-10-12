CREATE TABLE [incentives].[PendingPayment]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,	
    [AccountId] BIGINT NOT NULL, 
    [ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
    [DueDate] DATETIME2 NOT NULL, 
    [Amount] DECIMAL(9, 2) NOT NULL, 
    [CalculatedDate] DATETIME2 NOT NULL, 
    [PaymentMadeDate] DATETIME2 NULL,
    [PaymentPeriod] TINYINT NULL,	
	[PaymentYear] SMALLINT NULL
    CONSTRAINT FK_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].[ApprenticeshipIncentive](Id)
)
GO
CREATE CLUSTERED INDEX IX_PendingPayment ON [incentives].[PendingPayment] (AccountId)
GO

