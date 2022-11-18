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
select 
	PendingPaymentId,
	PeriodNumber, 
    PaymentYear,
	ISNULL(HasLearningRecord, 0) AS HasLearningRecord,
	ISNULL(IsInLearning, 0) AS IsInLearning,
	ISNULL(HasDaysInLearning, 0) AS HasDaysInLearning,
	ISNULL(HasNoDataLocks, 0) AS HasNoDataLocks,
	ISNULL(HasBankDetails, 0) AS HasBankDetails,
	ISNULL(PaymentsNotPaused, 0) AS PaymentsNotPaused,	
	ISNULL(HasIlrSubmission, 0) AS HasIlrSubmission,
	ISNULL(HasSignedMinVersion, 0) AS HasSignedMinVersion,
	ISNULL(LearnerMatchSuccessful, 0) AS LearnerMatchSuccessful,
	ISNULL(EmployedAtStartOfApprenticeship, 0) AS EmployedAtStartOfApprenticeship,
	ISNULL(EmployedBeforeSchemeStarted, 0) AS EmployedBeforeSchemeStarted,
	ISNULL(BlockedForPayments, 0) AS BlockedForPayments,
	IIF(EarningType = 'FirstPayment' , 1, ISNULL(EmployedAt365Days, 0)) AS EmployedAt365Days
from
(
  select 
	PendingPaymentId,
	ppvr.PeriodNumber,
	ppvr.PaymentYear,
	step,  
	CASE ISNULL(OverrideResult, 0)
		WHEN 1 THEN 1
		ELSE result
	END AS result,
	pp.EarningType
	from [incentives].[PendingPaymentValidationResult] ppvr inner join LatestPeriod 
		on ppvr.PeriodNumber = LatestPeriod.PeriodNumber and ppvr.PaymentYear = LatestPeriod.PaymentYear		
	inner join [incentives].[PendingPayment] pp 		
			on pp.id=ppvr.PendingPaymentId
) d
pivot
(
  max(result)
  for step in (
	HasBankDetails,
	BlockedForPayments,
	LearnerMatchSuccessful,
	HasIlrSubmission,
	HasLearningRecord,
	IsInLearning,
	HasNoDataLocks,
	HasDaysInLearning,
	PaymentsNotPaused,
	HasSignedMinVersion,
	EmployedAtStartOfApprenticeship,
	EmployedBeforeSchemeStarted,
	EmployedAt365Days,
	HasNoUnsentClawbacks)
) piv
)
select count(distinct pendingpaymentId) as [(Paste into cell A3 on tab {YTD Validation}) CountOfPayments], 
  HasLearningRecord,
  IsInLearning, 
  HasDaysInLearning, 
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused,
  '' as HasNoUnsentClawbacks, --Just used to space out the output in the excel template,
  HasIlrSubmission,
  HasSignedMinVersion,
  LearnerMatchSuccessful,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  EmployedAt365Days,
  count(distinct a.[AccountLegalEntityId]) as [AccountLegalEntityId],  
  SUM(IIF(pp.EarningType = 'FirstPayment', pp.amount, 0)) AS FirstEarningAmount,
  SUM(IIF(pp.EarningType = 'SecondPayment', pp.amount, 0)) AS SecondEarningAmount,
  SUM(ISNULL(pp.amount, 0)) as TottalEarningAmount
from PendingPaymentValidations ppv
	inner join [incentives].[PendingPayment] pp 
		on pp.id=ppv.PendingPaymentId
	inner join [dbo].[Accounts] a on pp.AccountLegalEntityId=a.AccountLegalEntityId
group by   
  HasLearningRecord, 
  IsInLearning, 
  HasDaysInLearning, 
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused, 
  HasIlrSubmission,   
  HasSignedMinVersion,
  LearnerMatchSuccessful,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  EmployedAt365Days
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
  BlockedForPayments desc,
  EmployedAt365Days desc
 