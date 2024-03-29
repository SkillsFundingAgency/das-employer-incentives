﻿CREATE TABLE [incentives].[ClawbackPayment]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
	[AccountId] BIGINT NOT NULL,	
	[AccountLegalEntityId] BIGINT NULL, 
	[Amount] DECIMAL(9, 2) NOT NULL, 
	[DateClawbackCreated] DATETIME2 NOT NULL, 
	[DateClawbackSent] DATETIME2 NULL, 
	[CollectionPeriod] TINYINT NULL,
	[CollectionPeriodYear] SMALLINT NULL,
	[SubNominalCode] INT NOT NULL,
	[PaymentId] UNIQUEIDENTIFIER NOT NULL,
	[VrfVendorId] NVARCHAR(100) NULL,
    CONSTRAINT FK_ClawbackPayment_ApprenticeshipIncentive FOREIGN KEY (ApprenticeshipIncentiveId) REFERENCES [incentives].ApprenticeshipIncentive(Id),
	CONSTRAINT FK_ClawbackPayment_PendingPayment FOREIGN KEY (PendingPaymentId) REFERENCES [incentives].PendingPayment(Id),
	CONSTRAINT FK_ClawbackPayment_Payment FOREIGN KEY (PaymentId) REFERENCES [incentives].Payment(Id)
)
GO
CREATE CLUSTERED INDEX IX_ClawbackPayment_DateClawbackSent ON [incentives].[ClawbackPayment] ([DateClawbackSent], [AccountLegalEntityId])
GO
CREATE UNIQUE INDEX IX_ClawbackPayment_ApprenticeshipIncentiveId ON [incentives].[ClawbackPayment] (ApprenticeshipIncentiveId, PendingPaymentId)
GO
CREATE INDEX IX_ClawbackPayment_PaymentId ON [incentives].[ClawbackPayment] (PaymentId)
GO