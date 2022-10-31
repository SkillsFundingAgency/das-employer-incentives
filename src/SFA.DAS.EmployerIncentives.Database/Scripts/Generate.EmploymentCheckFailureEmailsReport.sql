DECLARE @PeriodNumber TINYINT
DECLARE @PaymentYear SMALLINT

-- Update period & payment year here
SET @PeriodNumber = 2
SET @PaymentYear = 2223

-- Update period & payment year here

DECLARE @EmployedAtStartOfApprenticeshipFailures TABLE 
(
	uln bigint
)

DECLARE @EmployedBeforeSchemeStartedFailures TABLE 
(
	uln bigint
)

-- Pending payments that have failed EC validation in previous runs
INSERT INTO @EmployedAtStartOfApprenticeshipFailures
SELECT DISTINCT ULN
FROM incentives.PendingPaymentValidationResult ppvr
INNER JOIN [incentives].[PendingPayment] pp On ppvr.PendingPaymentId = pp.Id
INNER JOIN [incentives].[Learner] as l on pp.ApprenticeshipIncentiveID = l.ApprenticeshipIncentiveID
WHERE Step = 'EmployedAtStartOfApprenticeship' 
AND Result = 0 
AND ppvr.PeriodNumber < @PeriodNumber 
AND ppvr.PaymentYear = @PaymentYear
union
SELECT DISTINCT ULN
FROM incentives.PendingPaymentValidationResult ppvr
INNER JOIN [incentives].[PendingPayment] pp On ppvr.PendingPaymentId = pp.Id
INNER JOIN [incentives].[Learner] as l on pp.ApprenticeshipIncentiveID = l.ApprenticeshipIncentiveID
WHERE Step = 'EmployedAtStartOfApprenticeship' 
AND Result = 0 
AND ppvr.PaymentYear < @PaymentYear


INSERT INTO @EmployedBeforeSchemeStartedFailures
SELECT DISTINCT ULN
FROM incentives.PendingPaymentValidationResult ppvr
INNER JOIN [incentives].[PendingPayment] pp On ppvr.PendingPaymentId = pp.Id
INNER JOIN [incentives].[Learner] as l on pp.ApprenticeshipIncentiveID = l.ApprenticeshipIncentiveID
WHERE Step = 'EmployedBeforeSchemeStarted' 
AND Result = 0 
AND ppvr.PeriodNumber < @PeriodNumber 
AND ppvr.PaymentYear = @PaymentYear
Union
SELECT DISTINCT ULN
FROM incentives.PendingPaymentValidationResult ppvr
INNER JOIN [incentives].[PendingPayment] pp On ppvr.PendingPaymentId = pp.Id
INNER JOIN [incentives].[Learner] as l on pp.ApprenticeshipIncentiveID = l.ApprenticeshipIncentiveID
WHERE Step = 'EmployedBeforeSchemeStarted' 
AND Result = 0 
AND ppvr.PaymentYear < @PaymentYear

-- Failed 1 or Both
SELECT DISTINCT x.ULN, x.UKPRN, a.LegalEntityName, x.SubmittedByEmail, SUBSTRING(ia.SubmittedByName, 0, CHARINDEX(' ', ia.SubmittedByName)) as [First Name], TRIM(SUBSTRING(ia.SubmittedByName, CHARINDEX(' ', ia.SubmittedByName), LEN(ia.SubmittedByName)-CHARINDEX(' ', ia.SubmittedByName)+1)) AS [Last Name], '1OrBoth' AS 'CheckType' FROM
(SELECT 
	ai.UKPRN,
	ai.ULN,
	ai.Id,
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
	) AS EmployedAtStartOfApprenticeship,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC2 
	WHERE ppvrEC2.PendingPaymentId = pp.Id 
	AND ppvrEC2.Step = 'EmployedBeforeSchemeStarted' 
	AND ppvrEC2.PeriodNumber = @PeriodNumber 
	AND ppvrEC2.PaymentYear = @PaymentYear
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
AND x.ULN NOT IN (SELECT uln FROM @EmployedAtStartOfApprenticeshipFailures UNION SELECT uln FROM @EmployedBeforeSchemeStartedFailures)

-- Failed check 2
SELECT DISTINCT x.ULN, x.UKPRN, a.LegalEntityName, x.SubmittedByEmail, SUBSTRING(ia.SubmittedByName, 0, CHARINDEX(' ', ia.SubmittedByName)) as [First Name], TRIM(SUBSTRING(ia.SubmittedByName, CHARINDEX(' ', ia.SubmittedByName), LEN(ia.SubmittedByName)-CHARINDEX(' ', ia.SubmittedByName)+1)) AS [Last Name], '2nd' AS 'CheckType' FROM
(SELECT 
	ai.UKPRN,
	ai.ULN,
	ai.Id,
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
	) AS EmployedAtStartOfApprenticeship,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC2 
	WHERE ppvrEC2.PendingPaymentId = pp.Id 
	AND ppvrEC2.Step = 'EmployedBeforeSchemeStarted' 
	AND ppvrEC2.PeriodNumber = @PeriodNumber 
	AND ppvrEC2.PaymentYear = @PaymentYear
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
AND x.ULN NOT IN (SELECT uln FROM @EmployedAtStartOfApprenticeshipFailures UNION SELECT uln FROM @EmployedBeforeSchemeStartedFailures)

--EmployedAt365Days
SELECT DISTINCT x.ULN, x.UKPRN, a.LegalEntityName, x.SubmittedByEmail, SUBSTRING(ia.SubmittedByName, 0, CHARINDEX(' ', ia.SubmittedByName)) as [First Name], TRIM(SUBSTRING(ia.SubmittedByName, CHARINDEX(' ', ia.SubmittedByName), LEN(ia.SubmittedByName)-CHARINDEX(' ', ia.SubmittedByName)+1)) AS [Last Name], 'EmployedAt365Days' AS 'CheckType' FROM
(SELECT  
	ai.UKPRN,
	ai.ULN,
	ai.Id,
	ai.AccountLegalEntityId,
	ai.SubmittedByEmail,
	ai.IncentiveApplicationApprenticeshipId,
	ai.PausePayments,
	(
	SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC1 
	WHERE ppvrEC1.PendingPaymentId = pp.Id 
	AND ppvrEC1.Step = 'EmployedAt365Days' 
	AND ppvrEC1.PeriodNumber = @PeriodNumber 
	AND ppvrEC1.PaymentYear = @PaymentYear
	) AS EmployedAt365Days,
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
WHERE x.HasLearningRecord = 1 AND x.EmployedAt365Days = 0 AND x.PausePayments = 0
AND ((SELECT COUNT(1) FROM incentives.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL AND CreatedDateTime < '2022-10-11') = 4 -- ensure that initial and first and second 365 checks have been executed
OR (SELECT COUNT(1) FROM archive.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL AND CreatedDateTime < '2022-10-11') = 4)
AND x.ULN NOT IN (SELECT uln FROM @EmployedAtStartOfApprenticeshipFailures)
