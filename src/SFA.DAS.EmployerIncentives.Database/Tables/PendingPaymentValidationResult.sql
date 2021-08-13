CREATE TABLE [incentives].[PendingPaymentValidationResult]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,	
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [Step] NVARCHAR(50) NOT NULL, 
    [Result] BIT NOT NULL,
    [PeriodNumber] TINYINT NOT NULL, 
    [PaymentYear] SMALLINT NOT NULL, 
    [CreatedDateUTC] DATETIME2 NOT NULL
    CONSTRAINT FK_PendingPaymentId FOREIGN KEY (PendingPaymentId) REFERENCES [incentives].[PendingPayment](Id)
)
GO

CREATE CLUSTERED INDEX [IX_PendingPaymentsValidationResult_PendingPaymentId] ON [incentives].[PendingPaymentValidationResult]
(
	[PendingPaymentId] 
)
GO
