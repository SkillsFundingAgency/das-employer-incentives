CREATE VIEW [dbo].[BusinessPaymentsDashboard]
	AS 
SELECT
  --Volume results
  p.PaymentPeriod as [Period Number],p.PaymentYear as [Payment Year],
  count(pp.Id) as [Num Earnings],
  SUM(lastval.FailedValidation) as [Num EI Validation Failed],
  SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as [Num EI Payment Records],
  (SUM(lastval.FailedValidation)+cast(SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as float)*100)/nullif(count(pp.Id),0) as [% Earnings Handled],
  SUM(CASE WHEN p.PaidDate is not null THEN 1 else 0 END) as [Num BC Payments Sent], 
  isnull((cast(count(pp.Id) as float)/nullif(0,SUM(CASE WHEN p.PaidDate is not null THEN 1 else 0 END)))*100,0) as [% Earning to BC Payments],
  count(distinct p.AccountId) as [Num EI Accounts expecting payment],
  count(distinct CASE WHEN p.PaidDate is null THEN p.AccountId else 0 END) as [Num Accounts with missing BC payments],
  count(distinct a.VrfVendorId) as [Num Vendors expecting payment],
  count(distinct CASE WHEN p.PaidDate is null THEN a.VrfVendorId else null END) as [Num Vendors with missing BC payments],

  --Value results
  FORMAT(sum(pp.amount),'C','en-gb') as [EI Pending Payments Value], 
  FORMAT(sum(CASE WHEN lastval.FailedValidation = 1 then pp.Amount else 0 END),'C','en-gb') as [EI Validation Failed Value],
  FORMAT(sum(case when pp.paymentmadedate is not null then pp.amount else 0 end),'C','en-gb') as [EI Payments Made Value],
  FORMAT(sum(case when p.amount >= 0 then p.amount else 0 end),'C','en-gb') as [EI Positive Payments Value], 
  FORMAT(sum(case when p.amount < 0 then p.amount else 0 end),'C','en-gb') as [EI Negative Payments Value], 
  FORMAT(SUM(CASE WHEN p.PaidDate is not null THEN p.Amount else 0 END),'C','en-gb') as [BC Payments Sent Value]

FROM
  [incentives].[PendingPayment] pp
  left join [incentives].[Payment] p on p.PendingPaymentId = pp.Id
  left join [dbo].accounts a on a.id=p.AccountId
  left join (
			SELECT 
				pv1.[PendingPaymentId],
			  CASE WHEN pv1.result = 0 then 1 else 0 end as [FailedValidation]
			FROM [incentives].PendingPaymentValidationResult pv1 LEFT JOIN [incentives].PendingPaymentValidationResult pv2
			 ON (pv1.step = pv2.step 
				AND pv1.PaymentYear = pv2.PaymentYear 
				AND pv1.PeriodNumber = pv2.PeriodNumber 
				AND pv1.CreatedDateUTC < pv2.CreatedDateUTC)
			left join [incentives].[PendingPayment] pp on pp.Id = pv1.PendingPaymentId
			WHERE pv2.id IS NULL 
			group by pv1.[PendingPaymentId], CASE WHEN pv1.result = 0 then 1 else 0 end
		) lastval on lastval.[PendingPaymentId]=pp.[Id]
GROUP BY 
  p.PaymentPeriod,p.PaymentYear


