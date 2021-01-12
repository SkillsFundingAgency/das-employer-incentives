CREATE TABLE [incentives].[Payment]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
	[AccountId] BIGINT NOT NULL,
	[AccountLegalEntityId] BIGINT NOT NULL,
	[CalculatedDate] DATETIME2 NOT NULL,
	[PaidDate] DATETIME2 NULL,
	[SubNominalCode] INT NOT NULL,
	[PaymentPeriod] TINYINT NOT NULL,
	[PaymentYear] SMALLINT NOT NULL, 
	[Amount] DECIMAL(9, 2) NOT NULL
)
GO
CREATE INDEX IX_Payment_ApprenticeshipIncentiveId ON [incentives].[Payment] (ApprenticeshipIncentiveId)
GO
CREATE INDEX IX_Payment_PaidDate ON [incentives].[Payment] (PaidDate)