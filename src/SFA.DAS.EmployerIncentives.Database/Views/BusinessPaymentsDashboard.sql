CREATE VIEW [dbo].[BusinessPaymentsDashboard]
	AS 
SELECT
  --Volume results
	pp.PeriodNumber as [Earning Period Number],pp.PaymentYear as [Earning Year],
  count(pp.Id) as [Num Earnings],
  SUM(IIF (pv.validationResult = 0 or pv.validationResult is null,1,0)) as [Num Earnings Validation Failed],
  SUM(IIF (pv.validationResult = 1, 1, 0)) as [Num Earnings Validation Passed],
  SUM(IIF (pp.paymentmadedate is not null, 1, 0)) as [Num Earnings with made date],
  ((SUM(IIF (pv.validationResult = 0, 1, 0))+cast(SUM(IIF (pp.paymentmadedate is not null, 1, 0)) as float))/nullif(count(pp.Id),0))*100 as [% Earnings Handled],
  SUM(IIF (p.PaidDate is not null, 1, 0)) as [Num BC Payments Sent], 
  (SUM(IIF (p.PaidDate is not null, 1, 0))/nullif((cast(SUM( IIF(pp.paymentmadedate is not null, 1, 0)) as float)),0))*100 as [% Valid earning to BC Payments],
  count(distinct IIF( pv.validationResult = 1, p.AccountId, null )) as [Num Accounts with valid earnings],
  count(distinct IIF( p.PaidDate is null , p.AccountId, null)) as [Num Accounts with payments not sent to BC payments],
  count(distinct a.VrfVendorId) as [Num Vendors expecting payment],
  count(distinct IIF( p.PaidDate is null , a.VrfVendorId , null )) as [Num Vendors with missing BC payments],

  --Value results
  sum(pp.amount) as [Earnings Value], 
  sum(IIF(IIF( pv.validationResult = 0, 1, 0) = 1, pp.Amount, 0)) as [Validation Failed Value],
  sum(IIF( pp.paymentmadedate is not null, pp.amount, 0)) as [Payments Made Value],
  sum(IIF( p.amount >= 0 , p.amount, 0)) as [Positive Payments Value], 
  sum(IIF( p.amount < 0 , p.amount, 0)) as [Negative Payments Value], 
  SUM(IIF( p.PaidDate is not null, p.Amount, 0)) as [BC Payments Sent Value]


FROM
  [incentives].[PendingPayment] pp
  left join [incentives].[Payment] p on p.PendingPaymentId = pp.Id
  left join [dbo].accounts a on a.AccountLegalEntityId=pp.AccountLegalEntityId
  left join (select PendingPaymentID, min(cast(isnull(result,1) as int)) as validationResult,PeriodNumber,PaymentYear
			 from [incentives].PendingPaymentValidationResult ppvr
			 group by PendingPaymentID, PeriodNumber,PaymentYear) pv on pv.PendingPaymentId = pp.Id and pp.PeriodNumber<=pv.PeriodNumber and pp.PaymentYear=pv.PaymentYear
  left join (select min(createddateutc) FirstValidation, max(createddateutc) LastValidation, PeriodNumber, PaymentYear
			from	[incentives].[PendingPaymentValidationResult]
			group by PeriodNumber, PaymentYear) runtime on pp.PeriodNumber <= runtime.PeriodNumber and runtime.PaymentYear=pp.PaymentYear
  left join (  select min(paiddate) FirstPayment, max(paiddate) LastPayment, PaymentPeriod, PaymentYear
			from	[incentives].[Payment]
			group by PaymentPeriod, PaymentYear) payRun on pp.PeriodNumber = payrun.PaymentPeriod and pp.PaymentYear=payRun.PaymentYear
WHERE 	
  pp.CalculatedDate < runtime.FirstValidation
GROUP BY 
  pp.PeriodNumber ,pp.PaymentYear