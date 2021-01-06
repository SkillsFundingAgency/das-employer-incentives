CREATE VIEW [dbo].[BusinessPaymentsDashboard]
	AS 
SELECT
  --Volume results
  p.PaymentPeriod as [Period Number],p.PaymentYear as [Payment Year],
  count(pp.Id) as [Num Earnings],
  SUM(CASE WHEN pv1.result = 0 then 1 else 0 end) as [Num EI Validation Failed],
  SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as [Num EI Payment Records],
  ((SUM(CASE WHEN pv1.result = 0 then 1 else 0 end)+cast(SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as float))/nullif(count(pp.Id),0))*100 as [% Earnings Handled],
  SUM(CASE WHEN p.PaidDate is not null THEN 1 else 0 END) as [Num BC Payments Sent], 
  (SUM(CASE WHEN p.PaidDate is not null THEN 1 else 0 END)/nullif((cast(SUM(CASE WHEN pp.paymentmadedate is not null THEN 1 else 0 END) as float)),0))*100 as [% Valid earning to BC Payments],
  count(distinct p.AccountId) as [Num EI Accounts expecting payment],
  count(distinct CASE WHEN p.PaidDate is null THEN p.AccountId else 0 END) as [Num Accounts with missing BC payments],
  count(distinct a.VrfVendorId) as [Num Vendors expecting payment],
  count(distinct CASE WHEN p.PaidDate is null THEN a.VrfVendorId else null END) as [Num Vendors with missing BC payments],

  --Value results
  sum(pp.amount) as [EI Pending Payments Value], 
  sum(CASE WHEN CASE WHEN pv1.result = 0 then 1 else 0 end = 1 then pp.Amount else 0 END) as [EI Validation Failed Value],
  sum(case when pp.paymentmadedate is not null then pp.amount else 0 end) as [EI Payments Made Value],
  sum(case when p.amount >= 0 then p.amount else 0 end) as [EI Postive Payments Value], 
  sum(case when p.amount < 0 then p.amount else 0 end) as [EI Negative Payments Value], 
  SUM(CASE WHEN p.PaidDate is not null THEN p.Amount else 0 END) as [BC Payments Sent Value]
FROM
  [incentives].[PendingPayment] pp
  left join [incentives].[Payment] p on p.PendingPaymentId = pp.Id
  left join [dbo].accounts a on a.AccountLegalEntityId=p.AccountLegalEntityId
  left join [incentives].PendingPaymentValidationResult pv1  on pv1.PendingPaymentId = p.PendingPaymentId
GROUP BY 
  p.PaymentPeriod,p.PaymentYear

