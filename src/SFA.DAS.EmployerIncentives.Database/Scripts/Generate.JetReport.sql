-- Main 

SELECT 
     p.[PendingPaymentId] as PaymentRequestId
    ,p.[AccountId]
    ,p.[AccountLegalEntityId]
    ,cast(pp.DueDate as date) as [PendingPaymentDueDate]
    ,cast(p.[CalculatedDate] as date) as [EIProcessingDate]
    ,cast(p.[PaidDate] as date) as [SentToBCDate]
    ,p.[SubNominalCode]
    ,case 
        when p.[SubNominalCode] = 0 then 'Levy16To18' 
        when p.[SubNominalCode] = 1 then 'Levy19Plus' 
        when p.[SubNominalCode] = 2 then 'NonLevy16To18' 
        when p.[SubNominalCode] = 3 then 'NonLevy19Plus' 
     end as [SubNominalDesc]
    ,case
        when ai.[EmployerType] = 0 then 'Non-Levy'
        when ai.[EmployerType] = 1 then 'Levy'
     end as [ApprenticeshipEmployerTypeOnApproval]
    ,p.[PaymentPeriod]
    ,p.[PaymentYear]
    ,p.[Amount] as Amount
    ,p.VrfVendorId
    ,ai.ULN
    ,case
        when ai.Phase = 'Phase1' then 1
        when ai.Phase = 'Phase2' then 2
		when ai.Phase = 'Phase3' then 3
     end as [Phase]
    ,cast(ai.SubmittedDate as date) as [Application Date]
  FROM (
            select 
                p.[Id], 
                [ApprenticeshipIncentiveId],
                [PendingPaymentId],[AccountId],
                p.[AccountLegalEntityId],
                [CalculatedDate],
                [PaidDate],
                [SubNominalCode],
                [PaymentPeriod],
                [PaymentYear],
                [Amount],
				        ISNULL(p.[VrfVendorId], a.VrfVendorId) AS VrfVendorId
            from [incentives].[Payment] p
            INNER JOIN dbo.Accounts a 
            ON a.AccountLegalEntityId = p.AccountLegalEntityId
            union
            select 
                c.[Id], 
                [ApprenticeshipIncentiveId],
                [PendingPaymentId],[AccountId],
                c.[AccountLegalEntityId],
                [DateClawbackCreated] as [CalculatedDate],
                [DateClawBackSent] as [PaidDate],
                [SubNominalCode],
                [CollectionPeriod] as [PaymentPeriod],
                [CollectionPeriodYear] as [PaymentYear],
                [Amount],				
				        ISNULL(c.[VrfVendorId], a.VrfVendorId) AS VrfVendorId
            from [incentives].[ClawbackPayment] c
            INNER JOIN dbo.Accounts a
            ON a.AccountLegalEntityId = c.AccountLegalEntityId
        ) p
  left join [incentives].[PendingPayment] pp on pp.id=p.PendingPaymentId
  left join Accounts a on a.Id=p.AccountId and a.AccountLegalEntityId = p.AccountLegalEntityId
  left join [incentives].[ApprenticeshipIncentive] ai on ai.Id=p.ApprenticeshipIncentiveId
  where PaidDate is not null
  and p.VrfVendorId not in (''
  ----Replace with list of rejected payments from BC
  )
  order by PaymentYear desc, PaymentPeriod desc, SentToBCDate desc  

-- Validation Failures
;WITH LatestPeriod AS (
  SELECT TOP 1 PeriodNumber, PaymentYear FROM [BusinessGetMonthEndRuntimes] ORDER BY [LastValidation] DESC)
, ValidationsFailures as (
SELECT
	PendingPaymentId,
	PeriodNumber, 
    PaymentYear,
	ISNULL(HasBankDetails, 0) AS HasBankDetails,
	ISNULL(HasNoDataLocks, 0) AS HasNoDataLocks,
	ISNULL(PaymentsNotPaused, 0) AS PaymentsNotPaused,	
	ISNULL(EmployedAtStartOfApprenticeship, 0) AS EmployedAtStartOfApprenticeship,
	ISNULL(EmployedBeforeSchemeStarted, 0) AS EmployedBeforeSchemeStarted,
	ISNULL(BlockedForPayments, 0) AS BlockedForPayments,
	IIF(EarningType = 'FirstPayment' , 1, ISNULL(EmployedAt365Days, 0)) AS EmployedAt365Days
FROM
(
	SELECT
		PendingPaymentId,
		ppvr.PeriodNumber,
		ppvr.PaymentYear,
		step,  
		CASE ISNULL(OverrideResult, 0)
			WHEN 1 THEN 1
			ELSE result
		END AS result,
		pp.EarningType
	FROM [incentives].[PendingPaymentValidationResult] ppvr 
		INNER JOIN LatestPeriod 
			ON ppvr.PeriodNumber = LatestPeriod.PeriodNumber 
			AND ppvr.PaymentYear = LatestPeriod.PaymentYear		
		INNER JOIN [incentives].[PendingPayment] pp 		
			ON pp.id=ppvr.PendingPaymentId
) d
PIVOT
(
  MAX(result)
  FOR step IN (
	HasBankDetails,
	HasNoDataLocks,
	PaymentsNotPaused,
	EmployedAtStartOfApprenticeship,
	EmployedBeforeSchemeStarted,
	BlockedForPayments,
	EmployedAt365Days)
) piv
)
SELECT 
  ai.ULN,
  a.Id as AccountId,
  a.AccountLegalEntityId,  
  pp.PeriodNumber as PaymentPeriod,
  pp.PaymentYear as PaymentYear,
  CAST(pp.DueDate as date) as PendingPaymentDueDate,
  HasBankDetails, 
  HasNoDataLocks,   
  PaymentsNotPaused,
  IIF(HasBankDetails=0 AND PaymentsNotPaused=0, 0, 1) AS HasBankDetailsAndPaymentsNotPaused,
  IIF(HasBankDetails=0 AND HasNoDataLocks=0, 0, 1) AS HasBankDetailsAndHasNoDataLocks,
  IIF(PaymentsNotPaused=0 AND HasNoDataLocks=0, 0, 1) AS PaymentsNotPausedAndHasNoDataLocks,
  IIF(HasBankDetails=0 AND PaymentsNotPaused=0 AND HasNoDataLocks=0, 0, 1) AS HasBankDetailsAndPaymentsNotPausedAndHasNoDataLocks,
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  EmployedAt365Days,
  SUM(ISNULL(pp.amount, 0)) as Amount
FROM ValidationsFailures ppv
	INNER JOIN [incentives].[PendingPayment] pp 
		ON pp.id=ppv.PendingPaymentId
	INNER JOIN [incentives].[ApprenticeshipIncentive] ai 
		ON ai.Id=pp.ApprenticeshipIncentiveId
	INNER JOIN [dbo].[Accounts] a 
		ON pp.AccountLegalEntityId=a.AccountLegalEntityId	
WHERE	
	HasBankDetails = 0 
OR	HasNoDataLocks  = 0
OR	PaymentsNotPaused = 0
GROUP BY
  a.Id,
  a.AccountLegalEntityId,
  ai.ULN,
  pp.PeriodNumber,
  pp.PaymentYear,
  pp.DueDate,
  pp.EarningType,
  HasNoDataLocks, 
  HasBankDetails, 
  PaymentsNotPaused, 
  EmployedAtStartOfApprenticeship,
  EmployedBeforeSchemeStarted,
  BlockedForPayments,
  EmployedAt365Days
ORDER BY
  a.Id,
  a.AccountLegalEntityId,
  ai.ULN