DECLARE @PeriodNumber TINYINT
DECLARE @PaymentYear SMALLINT

-- Update period & payment year here
SET @PeriodNumber = 12
SET @PaymentYear = 2122
-- Update period & payment year here

DECLARE @EmployedAtStartOfApprenticeshipFailures TABLE 
(
	PendingPaymentId UNIQUEIDENTIFIER
)

DECLARE @EmployedBeforeSchemeStartedFailures TABLE 
(
	PendingPaymentId UNIQUEIDENTIFIER
)

-- Pending payments that have failed EC validation in previous runs
INSERT INTO @EmployedAtStartOfApprenticeshipFailures
SELECT DISTINCT PendingPaymentId 
FROM incentives.PendingPaymentValidationResult
WHERE Step = 'EmployedAtStartOfApprenticeship' 
AND Result = 0 
AND PeriodNumber <> @PeriodNumber 
OR PaymentYear <> @PaymentYear

INSERT INTO @EmployedBeforeSchemeStartedFailures
SELECT DISTINCT PendingPaymentId 
FROM incentives.PendingPaymentValidationResult
WHERE Step = 'EmployedBeforeSchemeStarted' 
AND Result = 0 
AND PeriodNumber <> @PeriodNumber 
OR PaymentYear <> @PaymentYear

-- Failed 1 or Both
SELECT DISTINCT x.SubmittedByEmail, SUBSTRING(ia.SubmittedByName, 0, CHARINDEX(' ', ia.SubmittedByName)) as [First Name], TRIM(SUBSTRING(ia.SubmittedByName, CHARINDEX(' ', ia.SubmittedByName), LEN(ia.SubmittedByName)-CHARINDEX(' ', ia.SubmittedByName)+1)) AS [Last Name], '1OrBoth' AS 'CheckType' FROM
(SELECT ai.Id,
	ai.AccountLegalEntityId,
	ai.SubmittedByEmail,
	ai.IncentiveApplicationApprenticeshipId,
	ai.PausePayments,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC1 
	WHERE ppvrEC1.PendingPaymentId = pp.Id 
	AND ppvrEC1.Step = 'EmployedAtStartOfApprenticeship' 
	AND ppvrEC1.PeriodNumber = @PeriodNumber 
	AND ppvrEC1.PaymentYear = @PaymentYear
	AND pp.Id NOT IN (SELECT PendingPaymentId FROM @EmployedAtStartOfApprenticeshipFailures)
	) AS EmployedAtStartOfApprenticeship,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC2 
	WHERE ppvrEC2.PendingPaymentId = pp.Id 
	AND ppvrEC2.Step = 'EmployedBeforeSchemeStarted' 
	AND ppvrEC2.PeriodNumber = @PeriodNumber 
	AND ppvrEC2.PaymentYear = @PaymentYear
	AND pp.Id NOT IN (SELECT PendingPaymentId FROM @EmployedBeforeSchemeStartedFailures)
	) AS EmployedBeforeSchemeStarted,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrLearn 
	WHERE ppvrLearn.PendingPaymentId = pp.Id 
	AND ppvrLearn.Step = 'HasLearningRecord' 
	AND ppvrLearn.PeriodNumber = @PeriodNumber 
	AND ppvrLearn.PaymentYear = @PaymentYear
	) AS HasLearningRecord
FROM incentives.ApprenticeshipIncentive ai
INNER JOIN incentives.PendingPayment pp ON pp.ApprenticeshipIncentiveId = ai.Id) x
INNER JOIN Accounts a ON a.AccountLegalEntityId = x.AccountLegalEntityId
INNER JOIN IncentiveApplicationApprenticeship iaa ON iaa.Id = x.IncentiveApplicationApprenticeshipId
INNER JOIN IncentiveApplication ia ON ia.Id = iaa.IncentiveApplicationId
WHERE x.HasLearningRecord = 1 AND x.EmployedBeforeSchemeStarted = 0 AND x.PausePayments = 0
AND ((SELECT COUNT(1) FROM incentives.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2
OR (SELECT COUNT(1) FROM archive.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2)

-- Failed check 2
SELECT DISTINCT x.SubmittedByEmail, SUBSTRING(ia.SubmittedByName, 0, CHARINDEX(' ', ia.SubmittedByName)) as [First Name], TRIM(SUBSTRING(ia.SubmittedByName, CHARINDEX(' ', ia.SubmittedByName), LEN(ia.SubmittedByName)-CHARINDEX(' ', ia.SubmittedByName)+1)) AS [Last Name], '2nd' AS 'CheckType' FROM
(SELECT ai.Id,
	ai.AccountLegalEntityId,
	ai.SubmittedByEmail,
	ai.IncentiveApplicationApprenticeshipId,
	ai.PausePayments,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC1 
	WHERE ppvrEC1.PendingPaymentId = pp.Id 
	AND ppvrEC1.Step = 'EmployedAtStartOfApprenticeship' 
	AND ppvrEC1.PeriodNumber = @PeriodNumber 
	AND ppvrEC1.PaymentYear = @PaymentYear
	AND pp.Id NOT IN (SELECT PendingPaymentId FROM @EmployedAtStartOfApprenticeshipFailures)
	) AS EmployedAtStartOfApprenticeship,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC2 
	WHERE ppvrEC2.PendingPaymentId = pp.Id 
	AND ppvrEC2.Step = 'EmployedBeforeSchemeStarted' 
	AND ppvrEC2.PeriodNumber = @PeriodNumber 
	AND ppvrEC2.PaymentYear = @PaymentYear
	AND pp.Id NOT IN (SELECT PendingPaymentId FROM @EmployedBeforeSchemeStartedFailures)
	) AS EmployedBeforeSchemeStarted,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrLearn 
	WHERE ppvrLearn.PendingPaymentId = pp.Id 
	AND ppvrLearn.Step = 'HasLearningRecord' 
	AND ppvrLearn.PeriodNumber = @PeriodNumber 
	AND ppvrLearn.PaymentYear = @PaymentYear
	) AS HasLearningRecord
FROM incentives.ApprenticeshipIncentive ai
INNER JOIN incentives.PendingPayment pp ON pp.ApprenticeshipIncentiveId = ai.Id) x
INNER JOIN Accounts a ON a.AccountLegalEntityId = x.AccountLegalEntityId
INNER JOIN IncentiveApplicationApprenticeship iaa ON iaa.Id = x.IncentiveApplicationApprenticeshipId
INNER JOIN IncentiveApplication ia ON ia.Id = iaa.IncentiveApplicationId
WHERE x.HasLearningRecord = 1 AND x.EmployedAtStartOfApprenticeship = 0 AND x.EmployedBeforeSchemeStarted = 1 AND x.PausePayments = 0
AND ((SELECT COUNT(1) FROM incentives.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2
OR (SELECT COUNT(1) FROM archive.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2)