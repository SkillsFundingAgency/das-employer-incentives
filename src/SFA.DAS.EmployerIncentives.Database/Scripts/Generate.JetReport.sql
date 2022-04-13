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
    ,a.VrfVendorId
    ,ai.ULN
    ,case
        when ai.Phase = 'Phase1' then 1
        when ai.Phase = 'Phase2' then 2
		when ai.Phase = 'Phase3' then 3
     end as [Phase]
    ,cast(ai.SubmittedDate as date) as [Application Date]
  FROM (
            select 
                [Id], 
                [ApprenticeshipIncentiveId],
                [PendingPaymentId],[AccountId],
                [AccountLegalEntityId],
                [CalculatedDate],
                [PaidDate],
                [SubNominalCode],
                [PaymentPeriod],
                [PaymentYear],
                [Amount] 
            from [incentives].[Payment] 
            union
            select 
                [Id], 
                [ApprenticeshipIncentiveId],
                [PendingPaymentId],[AccountId],
                [AccountLegalEntityId],
                [DateClawbackCreated] as [CalculatedDate],
                [DateClawBackSent] as [PaidDate],
                [SubNominalCode],
                [CollectionPeriod] as [PaymentPeriod],
                [CollectionPeriodYear] as [PaymentYear],
                [Amount] 
            from [incentives].[ClawbackPayment]
        ) p
  left join [incentives].[PendingPayment] pp on pp.id=p.PendingPaymentId
  left join Accounts a on a.Id=p.AccountId and a.AccountLegalEntityId = p.AccountLegalEntityId
  left join [incentives].[ApprenticeshipIncentive] ai on ai.Id=p.ApprenticeshipIncentiveId
  where PaidDate is not null
  and VrfVendorId not in (''
  ----Replace with list of rejected payments from BC
  )
  order by PaymentYear desc, PaymentPeriod desc, SentToBCDate desc  

-- Validation Failures

;WITH validationsFailures AS( 
SELECT 
	 a.Id as AccountId,
	 a.AccountLegalEntityId,
	 ai.ULN,
	 MAX(IIF(ppv.step='HasBankDetails' and ppv.result=1,1,0)) as HasBankDetails,
	 MAX(IIF(ppv.step='HasNoDataLocks' and ppv.result=1,1,0)) as HasNoDataLocks,
	 MAX(IIF(ppv.step='PaymentsNotPaused' and ppv.result=1,1,0)) as PaymentsNotPaused,
     MAX(IIF(ppv.step='EmployedAtStartOfApprenticeship' and ppv.result=1,1,0)) as EmployedAtStartOfApprenticeship,
     MAX(IIF(ppv.step='EmployedBeforeSchemeStarted' and ppv.result=1,1,0)) as EmployedBeforeSchemeStarted,
     MAX(IIF(ppv.step='BlockedForPayments' and ppv.result=1,1,0)) as BlockedForPayments,
	 pp.amount,
	 pp.PeriodNumber AS PaymentPeriod,
	 pp.PaymentYear,
	 pp.DueDate AS PendingPaymentDueDate
FROM
	 [incentives].[PendingPaymentValidationResult] ppv
LEFT JOIN [incentives].[PendingPayment] pp on pp.id=ppv.PendingPaymentId
LEFT JOIN [incentives].[ApprenticeshipIncentive] ai on ai.Id=pp.ApprenticeshipIncentiveId
left join [dbo].[Accounts] a on pp.AccountLegalEntityId=a.AccountLegalEntityId
GROUP BY 
	a.Id,
	a.AccountLegalEntityId,
	ai.ULN, 
	pp.amount,
	pp.PeriodNumber,
	pp.PaymentYear,
	pp.DueDate
)
SELECT
	ULN,
	AccountId,
	AccountLegalEntityId,
	PaymentPeriod,
	PaymentYear,
	CAST(PendingPaymentDueDate as date) as PendingPaymentDueDate,
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
	Amount
FROM
	validationsFailures vf
WHERE
	 HasBankDetails = 0 
	OR HasNoDataLocks  = 0
	OR PaymentsNotPaused = 0