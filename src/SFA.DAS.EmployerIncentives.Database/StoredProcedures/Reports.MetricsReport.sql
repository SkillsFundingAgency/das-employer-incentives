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
	SELECT TOP 1 PeriodNumber, PaymentYear FROM [BusinessGetMonthEndRuntimes] ORDER BY [LastValidation] DESC)
	,PendingPaymentValidations AS (
		SELECT 
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
			EmployedBeforeSchemeStarted DESC,
			BlockedForPayments DESC,
			EmployedAt365Days DESC)	AS 'Order',
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
		BlockedForPayments,
		EmployedAt365Days,
		COUNT(DISTINCT a.[AccountLegalEntityId]) AS [AccountLegalEntityId],
		SUM(IIF(pp.EarningType = 'FirstPayment', pp.amount, 0)) AS FirstEarningAmount,
		SUM(IIF(pp.EarningType = 'SecondPayment', pp.amount, 0)) AS SecondEarningAmount,
		SUM(ISNULL(pp.amount, 0)) as TotalEarningAmount
	FROM
		PendingPaymentValidations ppv LEFT JOIN [incentives].[PendingPayment] pp 
				ON pp.id=ppv.PendingPaymentId
			LEFT JOIN [dbo].[Accounts] a 
				ON pp.AccountLegalEntityId=a.AccountLegalEntityId
	GROUP BY
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


		
  