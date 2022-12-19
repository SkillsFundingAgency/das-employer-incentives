DECLARE @PeriodNumber TINYINT
DECLARE @PaymentYear SMALLINT
DECLARE @serviceRequestTaskId VARCHAR(50)

-- Update period & payment year here
SET @PeriodNumber = 1
SET @PaymentYear = 2223
-- Update period & payment year here
-- Update the service request ID for the Compliance pause request here
SET @serviceRequestTaskId = ''
-- Update the service request ID for the Compliance pause request here

DECLARE @CompliancePausedApplications TABLE
(
	ApprenticeshipIncentiveId UNIQUEIDENTIFIER
)

INSERT INTO @CompliancePausedApplications
(ApprenticeshipIncentiveId)
SELECT ai.Id
FROM incentives.ApprenticeshipIncentive ai
WHERE ai.IncentiveApplicationApprenticeshipId
IN
(
SELECT IncentiveApplicationApprenticeshipId
  FROM [dbo].[IncentiveApplicationStatusAudit]
  where process = 'PaymentsPaused'
  and ServiceRequestTaskId = @serviceRequestTaskId
)

-- Failed 1 or Both
SELECT x.AccountLegalEntityId, x.ULN, 'EmployedBeforeSchemeStarted' FROM
(SELECT 
    ai.Id,
    ai.AccountLegalEntityId,
	ai.ULN,
	ai.PausePayments,
	(SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC2 WHERE ppvrEC2.PendingPaymentId = pp.Id AND ppvrEC2.Step = 'EmployedBeforeSchemeStarted' AND ppvrEC2.PeriodNumber = @PeriodNumber AND ppvrEC2.PaymentYear = @PaymentYear) AS EmployedBeforeSchemeStarted,
	(SELECT Result FROM incentives.PendingPaymentValidationResult ppvrLearn WHERE ppvrLearn.PendingPaymentId = pp.Id AND ppvrLearn.Step = 'HasLearningRecord' AND ppvrLearn.PeriodNumber = @PeriodNumber AND ppvrLearn.PaymentYear = @PaymentYear) AS HasLearningRecord
FROM incentives.ApprenticeshipIncentive ai
INNER JOIN incentives.PendingPayment pp ON pp.ApprenticeshipIncentiveId = ai.Id) x
INNER JOIN @CompliancePausedApplications cba ON cba.ApprenticeshipIncentiveId = x.Id
WHERE x.HasLearningRecord = 1 AND x.EmployedBeforeSchemeStarted = 0 AND x.PausePayments = 1
AND ((SELECT COUNT(1) FROM incentives.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2
OR (SELECT COUNT(1) FROM archive.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2)

-- Failed check 2
SELECT x.AccountLegalEntityId, x.ULN, 'EmployedAtStartOfApprenticeship' FROM
(SELECT 
    ai.Id,
    ai.AccountLegalEntityId,
	ai.ULN,
	ai.PausePayments,
	(SELECT Result FROM incentives.PendingPaymentValidationResult ppvrEC1 WHERE ppvrEC1.PendingPaymentId = pp.Id AND ppvrEC1.Step = 'EmployedAtStartOfApprenticeship' AND ppvrEC1.PeriodNumber = @PeriodNumber AND ppvrEC1.PaymentYear = @PaymentYear) AS EmployedAtStartOfApprenticeship,
	(SELECT Result FROM incentives.PendingPaymentValidationResult ppvrLearn WHERE ppvrLearn.PendingPaymentId = pp.Id AND ppvrLearn.Step = 'HasLearningRecord' AND ppvrLearn.PeriodNumber = @PeriodNumber AND ppvrLearn.PaymentYear = @PaymentYear) AS HasLearningRecord
FROM incentives.ApprenticeshipIncentive ai
INNER JOIN incentives.PendingPayment pp ON pp.ApprenticeshipIncentiveId = ai.Id) x
INNER JOIN @CompliancePausedApplications cba ON cba.ApprenticeshipIncentiveId = x.Id
WHERE x.HasLearningRecord = 1 AND x.EmployedAtStartOfApprenticeship = 0 AND x.PausePayments = 1
AND ((SELECT COUNT(1) FROM incentives.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2
OR (SELECT COUNT(1) FROM archive.EmploymentCheck ec WHERE ec.ApprenticeshipIncentiveId = x.Id AND ec.Result IS NOT NULL) = 2)
