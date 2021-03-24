CREATE TABLE [archive].[PendingPaymentValidationResult]
(
    [PendingPaymentValidationResultId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,	
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [Step] NVARCHAR(20) NOT NULL, 
    [Result] BIT NOT NULL,
    [PeriodNumber] TINYINT NOT NULL, 
    [PaymentYear] SMALLINT NOT NULL, 
    [CreatedDateUTC] DATETIME2 NOT NULL,
	[ArchiveDateUTC] DATETIME2 NOT NULL,
    CONSTRAINT FK_ArchivePendingPaymentValidationResult_PendingPaymentId FOREIGN KEY (PendingPaymentId) REFERENCES [archive].[PendingPayment](PendingPaymentId)
)
GO

CREATE CLUSTERED INDEX [IX_ArchivePendingPaymentsValidationResult_Step] ON [archive].[PendingPaymentValidationResult]
(
	[Step] 
)
GO
CREATE INDEX IX_ArchivePendingPaymentsValidationResult_PendingPaymentId ON [archive].[PendingPaymentValidationResult] (PendingPaymentId)
GO 