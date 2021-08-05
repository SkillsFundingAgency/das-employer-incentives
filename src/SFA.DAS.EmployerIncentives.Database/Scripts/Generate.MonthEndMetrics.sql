/*** 

Payment run metrics report 

USAGE: Run the script below and then copy and paste each of the result sets into the Metrics Template.xlsx

       All cell definitions are just for the data and do not include the headers
***/



declare @CALC_DATE_TIME DATETIME = '2021-06-09T11:24:36'; -- exact start date time for IncentivePaymentOrchestrator_HttpStart 
declare @PERIOD int 

SELECT @PERIOD=periodnumber
  FROM [incentives].[CollectionCalendar]
  where active = 1
Select @period as [(Paste into cell B1) Current_Period], @CALC_DATE_TIME as 'Start of IncentivePaymentOrchestrator_HttpStart' 

-- Payments made (Paste into cell A6) 
SELECT PaymentYear as [(Paste into cell A6) PaymentYear],PaymentPeriod, count(*) as NumPayments,sum(p.[Amount]) as PaymentsAmount
  FROM [incentives].[Payment] p
  group by paymentyear,PaymentPeriod 
  order by paymentyear,PaymentPeriod

-- Current Period Totals (Paste into cell C22)
  ;SELECT Passed as [(Paste into cell C22) Passed], SUM(Amount) as PeriodValidation FROM
(SELECT [PendingPaymentId],
      MIN(CAST(Result AS INT)) AS Passed
  FROM [incentives].[PendingPaymentValidationResult]
  WHERE PeriodNumber = @PERIOD
  GROUP BY PendingPaymentId) x
  INNER JOIN incentives.PendingPayment pp ON pp.Id = x.PendingPaymentId
  GROUP By Passed order by passed desc

  -- Earnings (Paste into cell G6)
  ;SELECT PeriodNumber as [(Paste into cell G6) PeriodNumber], PaymentYear, sum(amount) as Amount, count(*) NumEarnings
  FROM [incentives].[PendingPayment] pp
  where 1=1
  and CalculatedDate <=  @CALC_DATE_TIME
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
  ;with PendingPaymentValidations as (
 select PendingPaymentId [PendingPaymentId], 
   max(iif(step='HasIlrSubmission',1,0)) as HasIlrSubmission,
   max(iif(step='HasLearningRecord',1,0)) as HasLearningRecord,
   max(iif(step='IsInLearning',1,0)) as IsInLearning,
   max(iif(step='HasDaysInLearning',1,0)) as HasDaysInLearning,
   max(iif(step='HasNoDataLocks',1,0)) as HasNoDataLocks,
   max(iif(step='HasBankDetails',1,0)) as HasBankDetails,
   max(iif(step='PaymentsNotPaused',1,0)) as PaymentsNotPaused,
   max(iif(step='HasSignedMinVersion',1,0)) as HasSignedMinVersion,
   max(iif(step='LearnerMatchSuccessful',1,0)) as LearnerMatchSuccessful,
   Result,
   PeriodNumber, 
   PaymentYear
 from [incentives].[PendingPaymentValidationResult] ppvr
 where ppvr.PeriodNumber = (select max(PeriodNumber) from [BusinessGetMonthEndRuntimes])
 and  ppvr.PaymentYear = (select max(PaymentYear) from [BusinessGetMonthEndRuntimes])
 and result = 1 
 group by PendingPaymentId, result, PeriodNumber, PaymentYear
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
  LearnerMatchSuccessful desc

-- YTD Validation (Paste underneath the Period Validation table on the [YTD Validation] tab) 
;with latestValidations as (
select max(ppv.periodnumber) MaxPeriod, 
  PendingPaymentId, 
  max(iif(step='HasIlrSubmission' and result=1,1,0)) as HasIlrSubmission,
  max(iif(step='HasLearningRecord' and result=1,1,0)) as HasLearningRecord,
    max(iif(step='IsInLearning' and result=1,1,0)) as IsInLearning,
    max(iif(step='HasDaysInLearning' and result=1,1,0)) as HasDaysInLearning,
    max(iif(step='HasNoDataLocks' and result=1,1,0)) as HasNoDataLocks,
    max(iif(step='HasBankDetails' and result=1,1,0)) as HasBankDetails,
    max(iif(step='PaymentsNotPaused' and result=1,1,0)) as PaymentsNotPaused,
    max((case when ppv.periodnumber <> 6 and ppv.paymentyear = 2021 then 1 else iif(step='HasNoUnsentClawbacks' and result=1,1,0) end)) as HasNoUnsentClawbacks,
    max((case when ppv.periodnumber < 9 and ppv.paymentyear = 2021 then 1 else iif(step='HasSignedMinVersion' and result=1,1,0) end)) as HasSignedMinVersion,
    max((case when ppv.periodnumber < 12 and ppv.paymentyear = 2021 then 1 else iif(step='LearnerMatchSuccessful' and result=1,1,0) end)) as LearnerMatchSuccessful,
    Amount,
    a.[AccountLegalEntityId] --Should only be one
from [incentives].[PendingPaymentValidationResult] ppv
left join [incentives].[PendingPayment] pp on pp.id=ppv.PendingPaymentId
left join [dbo].[Accounts] a on pp.AccountLegalEntityId=a.AccountLegalEntityId
group by PendingPaymentId , amount, a.[AccountLegalEntityId]
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
  LearnerMatchSuccessful
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
  LearnerMatchSuccessful desc