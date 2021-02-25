CREATE TABLE [incentives].[PendingPaymentValidationResultArchive]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,	
	[PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [Step] NVARCHAR(20) NOT NULL, 
    [Result] BIT NOT NULL,
    [PeriodNumber] TINYINT NOT NULL, 
    [PaymentYear] SMALLINT NOT NULL, 
    [CreatedDateUTC] DATETIME2 NOT NULL,
	[ArchiveDateUTC] DATETIME2 NOT NULL,
    CONSTRAINT FK_PendingPaymentValidationResultArchive_PendingPaymentId FOREIGN KEY (PendingPaymentId) REFERENCES [incentives].[PendingPaymentArchive](Id)
)
GO

CREATE CLUSTERED INDEX [IX_PendingPaymentsValidationResultArchive_Step] ON [incentives].[PendingPaymentValidationResultArchive]
(
	[Step] 
)
GO
CREATE INDEX IX_PendingPaymentsValidationResultArchive_PendingPaymentId ON [incentives].[PendingPaymentValidationResultArchive] (PendingPaymentId)
GO