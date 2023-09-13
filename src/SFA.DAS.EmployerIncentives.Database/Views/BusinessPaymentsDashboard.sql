CREATE VIEW [dbo].[BusinessPaymentsDashboard]
	AS 
SELECT
  --Volume results
	pp.PeriodNumber as [Earning Period Number],pp.PaymentYear as [Earning Year],
  count(pp.Id) as [Num Earnings],
  SUM(IIF (pv.ValidationResult = 0 or pv.ValidationResult is null,1,0)) as [Num Earnings Validation Failed],
  SUM(IIF (pv.ValidationResult = 1, 1, 0)) as [Num Earnings Validation Passed],
  SUM(IIF (pp.PaymentMadeDate is not null, 1, 0)) as [Num Earnings with made date],
  ((SUM(IIF (pv.ValidationResult = 0, 1, 0))+cast(SUM(IIF (pp.PaymentMadeDate is not null, 1, 0)) as float))/nullif(count(pp.Id),0))*100 as [% Earnings Handled],
  SUM(IIF (p.PaidDate is not null, 1, 0)) as [Num BC Payments Sent], 
  (SUM(IIF (p.PaidDate is not null, 1, 0))/nullif((cast(SUM( IIF(pp.PaymentMadeDate is not null, 1, 0)) as float)),0))*100 as [% Valid earning to BC Payments],
  count(distinct IIF( pv.ValidationResult = 1, p.AccountId, null )) as [Num Accounts with valid earnings],
  count(distinct IIF( p.PaidDate is null , p.AccountId, null)) as [Num Accounts with payments not sent to BC payments],
  count(distinct a.VrfVendorId) as [Num Vendors expecting payment],
  count(distinct IIF( p.PaidDate is null , a.VrfVendorId , null )) as [Num Vendors with missing BC payments],

  --Value results
  sum(pp.Amount) as [Earnings Value], 
  sum(IIF(IIF( pv.ValidationResult = 0, 1, 0) = 1, pp.Amount, 0)) as [Validation Failed Value],
  sum(IIF( pp.PaymentMadeDate is not null, pp.Amount, 0)) as [Payments Made Value],
  sum(IIF( p.Amount >= 0 , p.Amount, 0)) as [Positive Payments Value], 
  sum(IIF( p.Amount < 0 , p.Amount, 0)) as [Negative Payments Value], 
  SUM(IIF( p.PaidDate is not null, p.Amount, 0)) as [BC Payments Sent Value]


FROM
  [incentives].[PendingPayment] pp
  left join [incentives].[Payment] p on p.PendingPaymentId = pp.Id
  left join [dbo].Accounts a on a.AccountLegalEntityId=pp.AccountLegalEntityId
  left join (select PendingPaymentId, min(cast(isnull(Result,1) as int)) as ValidationResult,PeriodNumber,PaymentYear
			 from [incentives].PendingPaymentValidationResult ppvr
			 group by PendingPaymentId, PeriodNumber,PaymentYear) pv on pv.PendingPaymentId = pp.Id and pp.PeriodNumber<=pv.PeriodNumber and pp.PaymentYear=pv.PaymentYear
  left join (select min(CreatedDateUTC) FirstValidation, max(createddateutc) LastValidation, PeriodNumber, PaymentYear
			from	[incentives].[PendingPaymentValidationResult]
			group by PeriodNumber, PaymentYear) runtime on pp.PeriodNumber <= runtime.PeriodNumber and runtime.PaymentYear=pp.PaymentYear
  left join (  select min(PaidDate) FirstPayment, max(PaidDate) LastPayment, PaymentPeriod, PaymentYear
			from	[incentives].[Payment]
			group by PaymentPeriod, PaymentYear) payRun on pp.PeriodNumber = payrun.PaymentPeriod and pp.PaymentYear=payRun.PaymentYear
WHERE 	
  pp.CalculatedDate < runtime.FirstValidation
GROUP BY 
  pp.PeriodNumber,
  pp.PaymentYear