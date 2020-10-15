CREATE TABLE [incentives].[PendingPaymentValidationResult]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,	
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [ValidationStep] NVARCHAR(20) NOT NULL, 
    [ValidationResult] BIT NOT NULL,
    [CollectionPeriodMonth] TINYINT NOT NULL, 
    [CollectionPeriodYear] SMALLINT NOT NULL, 
    [CollectionDateUTC] DATETIME2 NOT NULL
    CONSTRAINT FK_PendingPaymentId FOREIGN KEY (PendingPaymentId) REFERENCES [incentives].[PendingPayment](Id)
)
GO

CREATE CLUSTERED INDEX [IX_PendingPaymentsValidationResult_PendingPaymentId] ON [incentives].[PendingPaymentValidationResult]
(
	[PendingPaymentId] 
)
GO
