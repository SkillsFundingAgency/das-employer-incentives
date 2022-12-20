CREATE VIEW [dbo].[BusinessGetMonthEndRuntimes]
	AS
  select min(CreatedDateUTC) FirstValidation, max(CreatedDateUTC) LastValidation, PeriodNumber, PaymentYear
			from	[incentives].[PendingPaymentValidationResult]
			group by PeriodNumber, PaymentYear
