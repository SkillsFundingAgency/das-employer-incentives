/*** 

Payment run metrics report 

USAGE: Run the script below and then copy and paste each of the result sets into the Metrics Template.xlsx

       All cell definitions are just for the data and do not include the headers
***/



declare @Period int 
declare @AcademicYear int

SELECT @Period=PeriodNumber, @AcademicYear=AcademicYear
  FROM [incentives].[CollectionCalendar]
  where active = 1
Select @Period as [(Paste into cell B1) Current_Period]

-- Payments made (Paste into cell A6) 
SELECT PaymentYear as [(Paste into cell A6) PaymentYear],PaymentPeriod, count(*) as NumPayments,sum(p.[Amount]) as PaymentsAmount
  FROM [incentives].[Payment] p
  group by paymentyear,PaymentPeriod 
  order by paymentyear,PaymentPeriod

-- Current Period Totals (Paste into cell C22)
  ;SELECT Passed as [(Paste into cell C22) Passed], SUM(Amount) as PeriodValidation FROM
(SELECT [PendingPaymentId],
      MIN(CAST((CASE OverrideResult
WHEN 1 THEN 1
ELSE Result
END) AS INT)) AS Passed
  FROM [incentives].[PendingPaymentValidationResult]
  WHERE PeriodNumber = @PERIOD AND PaymentYear = @AcademicYear
  GROUP BY PendingPaymentId) x
  INNER JOIN incentives.PendingPayment pp ON pp.Id = x.PendingPaymentId
  GROUP By Passed order by passed desc

-- YTD Valid Records (Paste into cell E22)
;SELECT SUM(Amount) AS [(Paste into cell E22) Valid YTD]
FROM incentives.Payment
WHERE PaymentYear = @AcademicYear
AND PaidDate IS NOT NULL

-- YTD Invalid Records (Paste into cell E23)
;SELECT SUM(Amount) AS [(Paste into cell E23) Invalid YTD]
FROM
(	
	SELECT SUM(Amount) AS Amount -- Total amount of all pending payments that have been validated this academic year
	FROM incentives.PendingPayment
	WHERE Id 
	IN
	(
	SELECT DISTINCT PendingPaymentId FROM incentives.PendingPaymentValidationResult
	WHERE PaymentYear = @AcademicYear	
	)
	UNION ALL
	SELECT (-1 * SUM(Amount)) AS Amount -- Amount of all payments for this academic year (subtracted from above total)
	FROM incentives.Payment
	WHERE PaymentYear = @AcademicYear
	AND PaidDate IS NOT NULL
)
AS [(Paste into cell E23) Invalid YTD]

  -- Earnings (Paste into cell G6)
  ;SELECT PeriodNumber as [(Paste into cell G6) PeriodNumber], PaymentYear, sum(amount) as Amount, count(*) NumEarnings
  FROM [incentives].[PendingPayment] pp
  where 1=1
  group by PeriodNumber, PaymentYear
  order by PaymentYear ,PeriodNumber

  -- Clawbacks (Paste into cell C32)
 ; SELECT 'Sent' as [(Paste into cell C32) Status],sum(amount)
  FROM [incentives].[ClawbackPayment]
  WHERE [DateClawbackSent] is not null
  union
  SELECT 'Unsent',sum(amount)
  FROM [incentives].[ClawbackPayment]
  WHERE [DateClawbackSent] is  null

  -- Validations (Current period) (Paste into cell A3 on tab [YTD Validation])
;with LatestPeriod as (
  select top 1 PeriodNumber, PaymentYear from [BusinessGetMonthEndRuntimes] order by [LastValidation] desc)
,PendingPaymentValidations as (
 select PendingPaymentId [PendingPaymentId], 
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasIlrSubmission',1,0)) 
	END
   as HasIlrSubmission,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasLearningRecord',1,0))
	END
    as HasLearningRecord,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='IsInLearning',1,0)) 
	END
   as IsInLearning,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasDaysInLearning',1,0)) 
	END
   as HasDaysInLearning,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasNoDataLocks',1,0)) 
	END
   as HasNoDataLocks,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasBankDetails',1,0)) 
	END
   as HasBankDetails,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='PaymentsNotPaused',1,0)) 
	END
   as PaymentsNotPaused,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasSignedMinVersion',1,0)) 
	END
   as HasSignedMinVersion,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='LearnerMatchSuccessful',1,0)) 
	END
   as LearnerMatchSuccessful,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='EmployedAtStartOfApprenticeship',1,0))
	END
    as EmployedAtStartOfApprenticeship,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='EmployedBeforeSchemeStarted',1,0)) 
	END
   as EmployedBeforeSchemeStarted,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='EmployedBeforeSchemeStarted',1,0)) 
	END
   as EmployedBeforeSchemeStarted,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='BlockedForPayments',1,0)) 
	END
   as BlockedForPayments,
   Result,
   ppvr.PeriodNumber, 
   ppvr.PaymentYear
 from [incentives].[PendingPaymentValidationResult] ppvr
 inner join LatestPeriod on ppvr.PeriodNumber = LatestPeriod.PeriodNumber and ppvr.PaymentYear = LatestPeriod.PaymentYear
 and result = 1 
 group by PendingPaymentId, ppvr.Result, ppvr.OverrideResult, ppvr.PeriodNumber, ppvr.PaymentYear 
)
select count(distinct pendingpaymentId) as [(Paste into cell A3 on tab {YTD Validation}) CountOfPayments], 
  HasLearningRecord,
  IsInLearning, 
  HasDaysInLearning, 
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused,
  '' as HasNoUnsentClawbacks, --Just used to space out the putput in the excel template,
  HasIlrSubmission,
  HasSignedMinVersion,
  LearnerMatchSuccessful,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  count(distinct a.[AccountLegalEntityId]) as [AccountLegalEntityId],
  sum(pp.amount) as EarningAmount
from PendingPaymentValidations ppv
left join [incentives].[PendingPayment] pp on pp.id=ppv.PendingPaymentId
left join [dbo].[Accounts] a on pp.AccountLegalEntityId=a.AccountLegalEntityId
group by HasIlrSubmission, 
  HasLearningRecord, 
  IsInLearning, 
  HasDaysInLearning, 
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused, 
  HasSignedMinVersion,
  LearnerMatchSuccessful,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  Result
    order by 
  HasLearningRecord desc, 
  IsInLearning desc, 
  HasDaysInLearning desc, 
  HasNoDataLocks desc, 
  HasBankDetails desc, 
  PaymentsNotPaused desc,
  HasIlrSubmission desc,
  HasSignedMinVersion desc,
  LearnerMatchSuccessful desc,
  EmployedAtStartOfApprenticeship desc,
  EmployedBeforeSchemeStarted desc,
  BlockedForPayments desc
-- YTD Validation (Paste underneath the Period Validation table on the [YTD Validation] tab) 
;with latestValidations as (
select max(ppv.periodnumber) MaxPeriod, 
  PendingPaymentId, 
  CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasIlrSubmission' and result=1,1,0))
	END
   as HasIlrSubmission,
  CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasLearningRecord' and result=1,1,0))
	END
  as HasLearningRecord,
  CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='IsInLearning' and result=1,1,0)) 
	END
    as IsInLearning,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasDaysInLearning' and result=1,1,0)) 
	END
    as HasDaysInLearning,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasNoDataLocks' and result=1,1,0)) 
	END
    as HasNoDataLocks,
    CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='HasBankDetails' and result=1,1,0)) 
	END
	as HasBankDetails,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max(iif(step='PaymentsNotPaused' and result=1,1,0))
	END
    as PaymentsNotPaused,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max((case when ppv.periodnumber <> 6 and ppv.paymentyear = 2021 then 1 else iif(step='HasNoUnsentClawbacks' and result=1,1,0) end)) 
	END
    as HasNoUnsentClawbacks,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max((case when ppv.periodnumber < 9 and ppv.paymentyear = 2021 then 1 else iif(step='HasSignedMinVersion' and result=1,1,0) end))
	END
    as HasSignedMinVersion,
    CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max((case when ppv.periodnumber < 12 and ppv.paymentyear = 2021 then 1 else iif(step='LearnerMatchSuccessful' and result=1,1,0) end)) 
	END
	as LearnerMatchSuccessful,
	CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max((case when ppv.periodnumber < 5 and ppv.paymentyear <= 2122 then 1 else iif(step='EmployedAtStartOfApprenticeship' and result=1,1,0) end))
	END
    as EmployedAtStartOfApprenticeship,
    CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max((case when ppv.periodnumber < 5 and ppv.paymentyear <= 2122 then 1 else iif(step='EmployedBeforeSchemeStarted' and result=1,1,0) end))
	END
	as EmployedBeforeSchemeStarted,
   CASE OverrideResult
	WHEN 1 THEN 1
	ELSE max((case when ppv.periodnumber < 10 and ppv.paymentyear <= 2122 then 1 else iif(step='BlockedForPayments' and result=1,1,0) end)) 
	END
	as BlockedForPayments,  
    Amount,
    a.[AccountLegalEntityId] --Should only be one
from [incentives].[PendingPaymentValidationResult] ppv
left join [incentives].[PendingPayment] pp on pp.id=ppv.PendingPaymentId
left join [dbo].[Accounts] a on pp.AccountLegalEntityId=a.AccountLegalEntityId
group by PendingPaymentId , amount, a.[AccountLegalEntityId], OverrideResult
)
select count(distinct pendingpaymentId) as [(Paste into cell A37 on tab {YTD Validation}) CountOfPayments], 
  HasLearningRecord, 
  IsInLearning, 
  HasDaysInLearning, 
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused,
  HasNoUnsentClawbacks,
  HasIlrSubmission,
  HasSignedMinVersion,
  LearnerMatchSuccessful,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  count(distinct [AccountLegalEntityId] ) as [AccountLegalEntityId],
  sum(amount) as EarningAmount
from latestValidations
group by
  HasLearningRecord, 
  IsInLearning, 
  HasDaysInLearning, 
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused,
  HasNoUnsentClawbacks,
  HasIlrSubmission,
  HasSignedMinVersion,
  LearnerMatchSuccessful,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments
order by 
  HasLearningRecord desc, 
  IsInLearning desc, 
  HasDaysInLearning desc, 
  HasNoDataLocks desc, 
  HasBankDetails desc, 
  PaymentsNotPaused desc,
  HasNoUnsentClawbacks desc,
  HasIlrSubmission desc,
  HasSignedMinVersion desc,
  LearnerMatchSuccessful desc,
  EmployedAtStartOfApprenticeship desc,
  EmployedBeforeSchemeStarted desc,
  BlockedForPayments desc