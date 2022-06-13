CREATE procedure [reports].[MetricsReport]

AS
	
	DECLARE 
		@Period			INT,
		@AcademicYear	VARCHAR(10)

	SELECT 
		@Period	= PeriodNumber, 
		@AcademicYear=AcademicYear
	FROM [incentives].[CollectionCalendar]
	WHERE active = 1

	SELECT 'CollectionPeriod', @Period AS 'Period', @AcademicYear AS 'AcademicYear'

	-- Payments made
	SELECT 
		ISNULL(NULL, 'PaymentsMade'),
		ROW_NUMBER() OVER (ORDER BY PaymentYear, PaymentPeriod)	AS 'Order',
		PaymentYear		AS 'Year',
		PaymentPeriod	AS	'Period', 
		ISNULL(COUNT(*), 0) as 'Number',
		ISNULL(SUM(p.[Amount]), 0) as 'Amount'
	  FROM 
		[incentives].[Payment] p
	  GROUP By 
		PaymentYear,
		PaymentPeriod 

	-- Earnings
	SELECT 
		ISNULL(NULL, 'Earning'),
		ROW_NUMBER() OVER (ORDER BY PaymentYear, PeriodNumber)	AS 'Order',
		PaymentYear		AS 'Year',
		PeriodNumber	AS 'Period',
		ISNULL(COUNT(*), 0)		AS 'Number',
		ISNULL(SUM(amount), 0)	AS 'Amount'
	  FROM 
		[incentives].[PendingPayment] pp
	  WHERE 1=1
	  GROUP BY
		PeriodNumber, 
		PaymentYear

	-- Clawbacks
	SELECT
		ISNULL(NULL, 'Clawbacks'),
		ISNULL(SentClawbacks.Amount, 0) AS 'Sent',
		ISNULL(UnsentClawbacks.Amount, 0) AS 'Unsent'
	FROM (
			SELECT 
				ISNULL(SUM(amount), 0) AS 'Amount'
			FROM 
				[incentives].[ClawbackPayment]
			WHERE
				[DateClawbackSent] is not null
			) AS SentClawbacks
		JOIN 
		(
		SELECT 
			ISNULL(SUM(amount), 0) AS 'Amount'
		FROM 
			[incentives].[ClawbackPayment]
		WHERE
			[DateClawbackSent] is null
		) AS UnsentClawbacks
		ON 1=1


	-- Validations (Current Period)
	;WITH LatestPeriod AS (
	SELECT TOP 1 
		PeriodNumber, 
		PaymentYear FROM [BusinessGetMonthEndRuntimes] ORDER BY [LastValidation] DESC),
		PendingPaymentValidations AS (
			SELECT
				PendingPaymentId [PendingPaymentId], 				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='HasIlrSubmission',1,0)) 
				END AS HasIlrSubmission,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='HasLearningRecord',1,0)) 
				END AS HasLearningRecord,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='IsInLearning',1,0)) 
				END AS IsInLearning,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='HasDaysInLearning',1,0)) 
				END AS HasDaysInLearning,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='HasNoDataLocks',1,0)) 
				END AS HasNoDataLocks,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='HasBankDetails',1,0)) 
				END AS HasBankDetails,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='PaymentsNotPaused',1,0)) 
				END AS PaymentsNotPaused,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='HasSignedMinVersion',1,0)) 
				END AS HasSignedMinVersion,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='LearnerMatchSuccessful',1,0)) 
				END AS LearnerMatchSuccessful,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='EmployedAtStartOfApprenticeship',1,0)) 
				END AS EmployedAtStartOfApprenticeship,				
				CASE OverrideResult
					WHEN 1 THEN 1
					ELSE MAX(IIF(step='EmployedBeforeSchemeStarted',1,0)) 
				END AS EmployedBeforeSchemeStarted,				
				Result,
				ppvr.PeriodNumber, 
				ppvr.PaymentYear
			FROM
				[incentives].[PendingPaymentValidationResult] ppvr INNER JOIN LatestPeriod 
					ON ppvr.PeriodNumber = LatestPeriod.PeriodNumber
					AND ppvr.PaymentYear = LatestPeriod.PaymentYear
					AND result = 1 
			GROUP BY
				PendingPaymentId, 
				ppvr.Result, 
				ppvr.OverrideResult,
				ppvr.PeriodNumber, 
				ppvr.PaymentYear
			)
	SELECT
		ISNULL(NULL, 'PeriodValidations'),
		ROW_NUMBER() OVER (ORDER BY
			HasLearningRecord DESC, 
			IsInLearning DESC, 
			HasDaysInLearning DESC, 
			HasNoDataLocks DESC, 
			HasBankDetails DESC, 
			PaymentsNotPaused DESC,
			HasIlrSubmission DESC,
			HasSignedMinVersion DESC,
			LearnerMatchSuccessful DESC,
			EmployedAtStartOfApprenticeship DESC,
			EmployedBeforeSchemeStarted DESC)	AS 'Order',
		COUNT(DISTINCT pendingpaymentId) AS 'CountOfPayments', 
		HasLearningRecord,
		IsInLearning, 
		HasDaysInLearning, 
		HasNoDataLocks, 
		HasBankDetails, 
		PaymentsNotPaused,
		0 AS HasNoUnsentClawbacks,
		HasIlrSubmission,
		HasSignedMinVersion,
		LearnerMatchSuccessful,
		EmployedAtStartOfApprenticeship,
		EmployedBeforeSchemeStarted,
		COUNT(DISTINCT a.[AccountLegalEntityId]) AS [AccountLegalEntityId],
		SUM(pp.amount) AS EarningAmount
	FROM
		PendingPaymentValidations ppv LEFT JOIN [incentives].[PendingPayment] pp 
			ON pp.id=ppv.PendingPaymentId
		LEFT JOIN [dbo].[Accounts] a 
			ON pp.AccountLegalEntityId=a.AccountLegalEntityId
	GROUP BY
		HasIlrSubmission, 
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
		Result


	-- Validations (YTD)
	;WITH latestValidations AS (
		SELECT 
			MAX(ppv.periodnumber) MaxPeriod, 
			PendingPaymentId, 
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='HasIlrSubmission' AND result=1,1,0))
			END AS HasIlrSubmission,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='HasLearningRecord' AND result=1,1,0))
			END AS HasLearningRecord,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='IsInLearning' AND result=1,1,0))
			END AS IsInLearning,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='HasDaysInLearning' AND result=1,1,0))
			END AS HasDaysInLearning,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='HasNoDataLocks' AND result=1,1,0))
			END AS HasNoDataLocks,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='HasBankDetails' AND result=1,1,0))
			END AS HasBankDetails,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE max(IIF(step='PaymentsNotPaused' AND result=1,1,0))
			END AS PaymentsNotPaused,			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE MAX((CASE WHEN ppv.periodnumber <> 6 AND ppv.paymentyear = 2021 THEN 1 ELSE IIF(step='HasNoUnsentClawbacks' AND result=1,1,0) END))
			END AS HasNoUnsentClawbacks,
			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE MAX((CASE WHEN ppv.periodnumber < 9 AND ppv.paymentyear = 2021 THEN 1 ELSE IIF(step='HasSignedMinVersion' AND result=1,1,0) END))
			END AS HasSignedMinVersion,
			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE MAX((CASE WHEN ppv.periodnumber < 12 AND ppv.paymentyear = 2021 THEN 1 ELSE IIF(step='LearnerMatchSuccessful' AND result=1,1,0) END))
			END AS LearnerMatchSuccessful,
			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE MAX((CASE WHEN ppv.periodnumber < 5 AND ppv.paymentyear <= 2122 THEN 1 ELSE IIF(step='EmployedAtStartOfApprenticeship' AND result=1,1,0) END))
			END AS EmployedAtStartOfApprenticeship,
			
			CASE OverrideResult
				WHEN 1 THEN 1
				ELSE MAX((CASE WHEN ppv.periodnumber < 5 AND ppv.paymentyear <= 2122 THEN 1 ELSE IIF(step='EmployedBeforeSchemeStarted' AND result=1,1,0) END))
			END AS EmployedBeforeSchemeStarted,
			ISNULL(Amount, 0) AS 'Amount',
			a.[AccountLegalEntityId] --Should only be one
		FROM [incentives].[PendingPaymentValidationResult] ppv LEFT JOIN [incentives].[PendingPayment] pp 
			ON pp.id=ppv.PendingPaymentId
		LEFT JOIN [dbo].[Accounts] a 
			ON pp.AccountLegalEntityId=a.AccountLegalEntityId
	GROUP BY
		PendingPaymentId, 
		amount, 
		a.[AccountLegalEntityId],
		OverrideResult
	)
	SELECT
		ISNULL(NULL, 'YtdValidations'),
		ROW_NUMBER() OVER (ORDER BY
			HasLearningRecord DESC, 
			IsInLearning DESC, 
			HasDaysInLearning DESC, 
			HasNoDataLocks DESC, 
			HasBankDetails DESC, 
			PaymentsNotPaused DESC,
			HasNoUnsentClawbacks DESC,
			HasIlrSubmission DESC,
			HasSignedMinVersion DESC,
			LearnerMatchSuccessful DESC,
			EmployedAtStartOfApprenticeship DESC,
			EmployedBeforeSchemeStarted DESC) AS 'Order',
		COUNT(DISTINCT pendingpaymentId) AS 'CountOfPayments', 
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
		ISNULL(COUNT(DISTINCT [AccountLegalEntityId]), 0) AS 'NumberOfAccountLegalEntityIds',
		ISNULL(SUM(Amount), 0) AS 'EarningAmount'
	FROM
		latestValidations
	GROUP BY
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
	  EmployedBeforeSchemeStarted


	-- Valid Validation records
	  SELECT 
		ISNULL(NULL, 'ValidValidation'),
		ISNULL(COUNT(pp.Id), 0)	AS 'Count',
		ISNULL(SUM(pp.Amount), 0)	AS 'PeriodAmount',
		(SELECT ISNULL(SUM(Amount), 0)
			FROM incentives.Payment
			WHERE PaymentYear = @AcademicYear
			AND PaidDate IS NOT NULL) AS 'YtdAmount'
	  FROM
	   incentives.PendingPayment pp INNER JOIN
	  (SELECT 		
			PendingPaymentId
		FROM [incentives].[PendingPaymentValidationResult]
		WHERE PeriodNumber = @Period
		AND PaymentYear = @AcademicYear
		GROUP BY
			PendingPaymentId	
		HAVING MIN(CAST((CASE OverrideResult
						WHEN 1 THEN 1
						ELSE Result
						END) AS INT)) = 1) As Passed		
		ON pp.Id = Passed.PendingPaymentId

	-- Invalid Validation records
	  SELECT 
		ISNULL(NULL, 'InvalidValidation'),
		ISNULL(COUNT(pp.Id), 0)	AS 'Count',
		ISNULL(SUM(pp.Amount), 0)	AS 'PeriodAmount',
		(
			SELECT ISNULL(SUM(Amount), 0)
			FROM
			(	
				SELECT ISNULL(SUM(Amount), 0) AS Amount -- Total amount of all pending payments that have been validated this academic year
				FROM incentives.PendingPayment
				WHERE Id 
				IN
				(
				SELECT DISTINCT PendingPaymentId FROM incentives.PendingPaymentValidationResult
				WHERE PaymentYear = @AcademicYear	
				)
				UNION ALL
				SELECT ISNULL((-1 * SUM(Amount)), 0) AS Amount -- Amount of all payments for this academic year (subtracted from above total)
				FROM incentives.Payment
				WHERE PaymentYear = @AcademicYear
				AND PaidDate IS NOT NULL
			) AS YTD
		)	AS 'YtdAmount'
	  FROM
	   incentives.PendingPayment pp INNER JOIN
	  (SELECT 		
			PendingPaymentId
		FROM [incentives].[PendingPaymentValidationResult]
		WHERE PeriodNumber = @Period 
		AND PaymentYear = @AcademicYear
		GROUP BY
			PendingPaymentId
		HAVING MIN(CAST(Result AS INT)) = 0) As Failed
		ON pp.Id = Failed.PendingPaymentId


		
  