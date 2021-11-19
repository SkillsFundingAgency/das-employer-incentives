/*
EI-1194 - RecoveryReport

This query will deliver the ability to produce an EI Recovery Report. This identifies employers (more specifically vendors) who have had clawbacks requested. For each listed vendor, the report will detail the earnings due for each vendor in each of the next 12 periods.

This report does not consider the current balance for the vendor on the vendor ledger. It only shows the forecasted earnings over the next 12 months for any vendor that has been sent a clawback as these are the only vendors in scope of the EI manual recovery process.

Identify all vendors which have at least one sent clawback		

List vendors in ascending vendor ID order						

For each vendor, identify amount of earnings for each of the next 12 periods (across all their account legal entities)				

Show a sum amount for earnings for next 3 periods				

Show a sum amount for earnings for next 12 periods				

Show a sum amount for all future earnings (first payment might not be until a future period so second payment would be after current period +12)				

Show a sum amount for all Compliance paused earnings				

Note : uses the current active period as the start period

*/
CREATE procedure [reports].[RecoveryReport]
AS
	DECLARE 
		@periodIdStart		INT,
		@3MonthPeriodId		INT,
		@12MonthPeriodId	INT,
		@columnNames NVARCHAR(MAX) = '',
		@vendorsSQL NVARCHAR(MAX) = '',
		@SQL NVARCHAR(MAX) = '',
		@QUERY NVARCHAR(MAX) = ''
		
	--SET @periodIdStart = (SELECT TOP 1 ID FROM [incentives].[CollectionCalendar] WHERE CensusDate >= GETDATE() ORDER BY ID) 
	SET @periodIdStart = (SELECT ID FROM [incentives].[CollectionCalendar] WHERE Active = 1) 
	SET @3MonthPeriodId = @periodIdStart + 2
	SET @12MonthPeriodId = @periodIdStart + 11

	SET @vendorsSQL = '(
	SELECT 
			DISTINCT
				cc.Id AS PeriodId,
				a.Id AS AccountId,		
				a.VrfVendorId,
				a.LegalEntityName,
				cc.AcademicYear,
				cc.PeriodNumber,
				cc.CensusDate,
				a.VrfCaseStatusLastUpdatedDateTime
			FROM 
					[dbo].[Accounts] a INNER JOIN [incentives].[ClawbackPayment] cp
						ON a.Id = AccountId
						AND a.AccountLegalEntityId = cp.AccountLegalEntityId
			CROSS JOIN [incentives].[CollectionCalendar] cc

			WHERE
				a.VrfVendorId IS NOT NULL 
			AND cc.Id BETWEEN ' + CONVERT(VARCHAR(10), @periodIdStart) + ' AND ' + CONVERT(VARCHAR(10), @12MonthPeriodId) + '
	) AS cb';
	  
	SELECT @columnNames += QUOTENAME(Id) + ','
	FROM 
		incentives.CollectionCalendar 
	WHERE Id BETWEEN @periodIdStart AND @12MonthPeriodId

	SET @columnNames = LEFT(@columnNames, LEN(@columnNames) -1)

	SET @Query =
	'
	SELECT
		cb.VrfVendorId,
		MAX(cb.VrfCaseStatusLastUpdatedDateTime) AS VrfCaseStatusLastUpdatedDateTime,
		cb.LegalEntityName,
		cb.PeriodId,
		cb.AcademicYear,
		cb.PeriodNumber,
		SUM(ISNULL(pp.Amount, 0)) AS Amount,
		cb.CensusDate,
		MAX(ISNULL(mnth3.AmountIn3Months, 0)) AS AmountIn3Months,
		MAX(ISNULL(mnth12.AmountIn12Months, 0)) AS AmountIn12Months,
		MAX(ISNULL(allFuture.AmountAllFuture, 0)) AS AmountAllFuture,
		MAX(ISNULL(paused.AmountPaused, 0)) AS AmountPaused	
	FROM 
		' + @vendorsSQL + ' LEFT OUTER JOIN [incentives].[PendingPayment] pp
		ON  pp.AccountId = cb.AccountId
		AND pp.PaymentYear = cb.AcademicYear
		AND pp.PeriodNumber = cb.PeriodNumber
		AND pp.ClawedBack = 0

		LEFT OUTER JOIN
		(
			SELECT 	
				cb.VrfVendorId,
				SUM(pp.Amount) as AmountIn3Months
			FROM 
				[incentives].[PendingPayment] pp INNER JOIN ' +  @vendorsSQL + '
					ON  pp.AccountId = cb.AccountId
					AND pp.PaymentYear = cb.AcademicYear
					AND pp.PeriodNumber = cb.PeriodNumber
					AND pp.ClawedBack = 0
					AND pp.PaymentMadeDate IS NULL
				
				INNER JOIN [incentives].[CollectionCalendar] cc
					ON pp.PaymentYear = cc.AcademicYear
					AND pp.PeriodNumber = cc.PeriodNumber
					AND cc.Id BETWEEN ' + CONVERT(VARCHAR(10), @periodIdStart) +  ' AND ' + CONVERT(VARCHAR(10), @3MonthPeriodId) + '
			GROUP BY
				cb.VrfVendorId
		) AS mnth3
		ON cb.VrfVendorId = mnth3.VrfVendorId

		LEFT OUTER JOIN
		(
			SELECT 	
				cb.VrfVendorId,
				SUM(pp.Amount) as AmountIn12Months
			FROM 
				[incentives].[PendingPayment] pp INNER JOIN ' +  @vendorsSQL + '
					ON  pp.AccountId = cb.AccountId
					AND pp.PaymentYear = cb.AcademicYear
					AND pp.PeriodNumber = cb.PeriodNumber
					AND pp.ClawedBack = 0
					AND pp.PaymentMadeDate IS NULL

				INNER JOIN [incentives].[CollectionCalendar] cc
					ON pp.PaymentYear = cc.AcademicYear
					AND pp.PeriodNumber = cc.PeriodNumber
					AND cc.Id BETWEEN ' + CONVERT(VARCHAR(10), @periodIdStart) + ' AND ' + CONVERT(VARCHAR(10), @12MonthPeriodId) + '
			GROUP BY
				cb.VrfVendorId
		) AS mnth12
		ON cb.VrfVendorId = mnth12.VrfVendorId	

		LEFT OUTER JOIN
		(
			SELECT 	
				a.VrfVendorId,
				SUM(pp.Amount) as AmountAllFuture
			FROM 
				[incentives].[PendingPayment] pp INNER JOIN 
				(
					SELECT DISTINCT
						a.id
					FROM 
						[dbo].[Accounts] a INNER JOIN [incentives].[ClawbackPayment] cp
							ON a.Id = AccountId
							AND a.AccountLegalEntityId = cp.AccountLegalEntityId
					WHERE
						a.VrfVendorId IS NOT NULL
				) as v
				ON pp.AccountId = v.Id
				AND pp.ClawedBack = 0
				AND pp.PaymentMadeDate IS NULL

				INNER JOIN [dbo].[Accounts] a
					ON a.Id = v.Id

				INNER JOIN [incentives].[CollectionCalendar] cc
					ON pp.PaymentYear = cc.AcademicYear
					AND pp.PeriodNumber = cc.PeriodNumber
					AND cc.Id >= ' + CONVERT(VARCHAR(10), @periodIdStart) + '
			GROUP BY
				a.VrfVendorId
		) AS allFuture
		ON cb.VrfVendorId = allFuture.VrfVendorId	

		LEFT OUTER JOIN
		(
			SELECT 	
				a.VrfVendorId,
				SUM(pp.Amount) as AmountPaused
			FROM 
				[incentives].[PendingPayment] pp INNER JOIN 
				(
					SELECT DISTINCT
						a.id
					FROM 
						[dbo].[Accounts] a INNER JOIN [incentives].[ClawbackPayment] cp
							ON a.Id = AccountId
							AND a.AccountLegalEntityId = cp.AccountLegalEntityId
					WHERE
						a.VrfVendorId IS NOT NULL
				) as v
				ON pp.AccountId = v.Id
				AND pp.ClawedBack = 0
				AND pp.PaymentMadeDate IS NULL

				INNER JOIN [dbo].[Accounts] a
					ON a.Id = v.Id

				INNER JOIN [incentives].[ApprenticeshipIncentive] ai
						ON ai.Id = pp.ApprenticeshipIncentiveId
						AND ai.PausePayments = 1

				INNER JOIN [incentives].[CollectionCalendar] cc
					ON pp.PaymentYear = cc.AcademicYear
					AND pp.PeriodNumber = cc.PeriodNumber
			GROUP BY
				a.VrfVendorId
		) AS paused
		ON cb.VrfVendorId = paused.VrfVendorId	

	GROUP BY
		cb.VrfVendorId,
		cb.LegalEntityName,
		cb.PeriodId,
		cb.AcademicYear,
		cb.PeriodNumber,
		cb.CensusDate
	'

	SET @SQL = 
	'SELECT 	
		Year = (SELECT AcademicYear FROM [incentives].[CollectionCalendar] WHERE Id = ' +  CONVERT(VARCHAR(10), @periodIdStart) + '),
		Month = (SELECT DATENAME(month, CensusDate) FROM [incentives].[CollectionCalendar] WHERE Id = ' +   CONVERT(VARCHAR(10), @periodIdStart) + '), 
		VrfVendorId,
		LegalEntityName,
		SUM(piv.' + QUOTENAME(@periodIdStart) + ')		AS EarningsMonth1,
		SUM(piv.' + QUOTENAME(@periodIdStart + 1) + ')	AS EarningsMonth2,
		SUM(piv.' + QUOTENAME(@periodIdStart + 2) + ')	AS EarningsMonth3,
		MAX(AmountIn3Months) AS EarningsNext3Months,
		SUM(piv.' + QUOTENAME(@periodIdStart + 3) + ')	AS EarningsMonth4,
		SUM(piv.' + QUOTENAME(@periodIdStart + 4) + ')	AS EarningsMonth5,
		SUM(piv.' + QUOTENAME(@periodIdStart + 5) + ')	AS EarningsMonth6,
		SUM(piv.' + QUOTENAME(@periodIdStart + 6) + ')	AS EarningsMonth7,
		SUM(piv.' + QUOTENAME(@periodIdStart + 7) + ')	AS EarningsMonth8,
		SUM(piv.' + QUOTENAME(@periodIdStart + 8) + ')	AS EarningsMonth9,
		SUM(piv.' + QUOTENAME(@periodIdStart + 9) + ')	AS EarningsMonth10,
		SUM(piv.' + QUOTENAME(@periodIdStart + 10) + ')	AS EarningsMonth11,
		SUM(piv.' + QUOTENAME(@periodIdStart + 11) + ')	AS EarningsMonth12,
		MAX(AmountIn12Months) AS EarningsIn12Months,
		MAX(AmountAllFuture) AS EarningsAllFuture,
		MAX(AmountPaused) AS EarningsPaused
	FROM  
	(
	 ' + @Query + '
	) src
	PIVOT
	(
	  sum(Amount)  
	  for PeriodId in (' + @columnNames + ')
	) piv
	GROUP BY
		piv.VrfVendorId,
		piv.LegalEntityName
	ORDER BY
		piv.VrfVendorId,
		piv.LegalEntityName
	'

	EXECUTE sp_executesql @SQL
