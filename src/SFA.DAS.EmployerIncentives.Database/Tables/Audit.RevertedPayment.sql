CREATE TABLE [audit].[RevertedPayment]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ApprenticeshipIncentiveId] UNIQUEIDENTIFIER NOT NULL,
	[PaymentId] UNIQUEIDENTIFIER NOT NULL,
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
	[PaymentPeriod] TINYINT NOT NULL,
	[PaymentYear] SMALLINT NOT NULL, 
	[Amount] DECIMAL(9, 2) NOT NULL,
	[CalculatedDate] DATETIME2 NOT NULL,
	[PaidDate] DATETIME2 NULL,
	[VrfVendorId] NVARCHAR(100) NULL,
	[ServiceRequestTaskId] NVARCHAR(100) NULL,
	[ServiceRequestDecisionReference] NVARCHAR(100) NULL,
	[ServiceRequestCreatedDate] DATETIME2 NULL,
	[CreatedDateTime] DATETIME2 NOT NULL
)
