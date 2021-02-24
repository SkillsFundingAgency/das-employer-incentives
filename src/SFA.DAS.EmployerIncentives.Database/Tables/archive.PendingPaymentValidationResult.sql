CREATE TABLE [archive].[PendingPaymentValidationResult]
(
    [PendingPaymentValidationResultId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
    [PendingPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [Step] NVARCHAR(20) NOT NULL,
    [Result] BIT NOT NULL,
    [PeriodNumber] TINYINT NOT NULL,
    [PaymentYear] SMALLINT NOT NULL,
    [CreatedDateUTC] DATETIME2 NOT NULL,
    [ArchivedDateUTC] DATETIME2 NOT NULL
)
GO