CREATE VIEW [dbo].[BusinessGetMonthEndRuntimes]
	AS
  select min(createddateutc) FirstValidation, max(createddateutc) LastValidation, PeriodNumber, PaymentYear
			from	[incentives].[PendingPaymentValidationResult]
			group by PeriodNumber, PaymentYear
