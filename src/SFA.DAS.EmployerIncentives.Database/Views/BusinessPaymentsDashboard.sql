CREATE VIEW [dbo].[BusinessPaymentsDashboard]
	AS 
SELECT
  --Volume results
	pp.PeriodNumber as [Earning Period Number],pp.PaymentYear as [Earning Year],
  count(pp.Id) as [Num Earnings],
  SUM(CASE WHEN pv.validationResult = 0 or pv.validationResult is null then 1 else 0 end) as [Num Earnings Validation Failed],
  SUM(CASE WHEN pv.validationResult = 1 then 1 else 0 end) as [Num Earnings Validation Passed],
  SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as [Num Earnings with made date],
  ((SUM(CASE WHEN pv.validationResult = 0 then 1 else 0 end)+cast(SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as float))/nullif(count(pp.Id),0))*100 as [% Earnings Handled],
  SUM(CASE WHEN p.PaidDate is not null THEN 1 else 0 END) as [Num BC Payments Sent], 
  (SUM(CASE WHEN p.PaidDate is not null THEN 1 else 0 END)/nullif((cast(SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as float)),0))*100 as [% Valid earning to BC Payments],
  count(distinct CASE WHEN pv.validationResult = 1 then p.AccountId end) as [Num Accounts with valid earnings],
  count(distinct CASE WHEN p.PaidDate is null THEN p.AccountId else 0 END) as [Num Accounts with payments not sent to BC payments],
  count(distinct a.VrfVendorId) as [Num Vendors expecting payment],
  count(distinct CASE WHEN p.PaidDate is null THEN a.VrfVendorId else null END) as [Num Vendors with missing BC payments],

  --Value results
  sum(pp.amount) as [Earnings Value], 
  sum(CASE WHEN CASE WHEN pv.validationResult = 0 then 1 else 0 end = 1 then pp.Amount else 0 END) as [Validation Failed Value],
  sum(case when pp.paymentmadedate is not null then pp.amount else 0 end) as [Payments Made Value],
  sum(case when p.amount >= 0 then p.amount else 0 end) as [Positive Payments Value], 
  sum(case when p.amount < 0 then p.amount else 0 end) as [Negative Payments Value], 
  SUM(CASE WHEN p.PaidDate is not null THEN p.Amount else 0 END) as [BC Payments Sent Value]


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