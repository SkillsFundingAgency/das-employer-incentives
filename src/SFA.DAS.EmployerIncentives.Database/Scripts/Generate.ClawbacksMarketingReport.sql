DECLARE @StartDate DATETIME
DECLARE @EndDate DATETIME

-- Adjust date ranges here
SET @StartDate = '2021-01-01'
SET @EndDate = '2024-02-29'
-- Adjust date ranges here

DECLARE @NewPaymentGenerated TABLE 
(
ULN BIGINT,
EarningType VARCHAR(255),
NewPaymentGenerated VARCHAR(10)
)

INSERT INTO @NewPaymentGenerated
SELECT ai.ULN, EarningType as [Payment],
CASE
WHEN COUNT (EarningType) > 1 THEN 'Yes' 
ELSE 'No'
END as 'NewPaymentGenerated'
FROM [incentives].[ApprenticeshipIncentive] ai
JOIN incentives.PendingPayment pp1 on ai.Id = pp1.ApprenticeshipIncentiveId
GROUP BY ULN, EarningType
ORDER BY ULN

SELECT 
ISNULL(cp.VrfVendorId, a.VrfVendorId) AS [Vendor No],
a.HashedLegalEntityId AS [Hashed Legal Entity ID],
a.LegalEntityName,
ia.SubmittedByEmail AS [Submitted by email],
ia.SubmittedByName AS [Submitted by name],
ai.ULN,
ISNULL(dil.NumberOfDaysInLearning, 0) AS [Number of days in learning],
ai.[Status] AS [EI Application status],
CASE
	WHEN ai.[Status] = 'Withdrawn' THEN 'Yes'
	WHEN ai.[Status] <> 'Withdrawn' AND pp.EarningType = 'FirstPayment' AND NumberOfDaysInLearning < 90 THEN 'Yes'
	WHEN ai.[Status] <> 'Withdrawn' AND pp.EarningType = 'SecondPayment' AND NumberOfDaysInLearning < 365 THEN 'Yes'
	WHEN ai.[Status] <> 'Withdrawn' AND pp.EarningType = 'FirstPayment' AND NumberOfDaysInLearning >= 90 THEN 'No'
	WHEN ai.[Status] <> 'Withdrawn' AND pp.EarningType = 'SecondPayment' AND NumberOfDaysInLearning >= 365 THEN 'No'
	ELSE 'Unknown'
END as [Clawback valid],
npg.NewPaymentGenerated AS [New Payment Generated],
LOWER(REPLACE(cp.Id, '-', '')) AS [Payment Request ID],
cp.Amount AS [Clawback amount],
p.PaidDate AS [Payment made date],
cp.DateClawbackSent AS [Clawback sent date],
CASE
	WHEN ec1.Result = 1  
			THEN 'Failed Check 1'
	WHEN ec2.Result = 0
			THEN 'Failed Check 2'
	WHEN ec3.Result = 0 
			THEN 'Failed 365 Check 2'
	WHEN ec1.Result IS NULL
			THEN NULL
			ELSE 'Pass'
END as [EMV status]
FROM
incentives.ClawbackPayment cp
INNER JOIN incentives.PendingPayment pp
ON pp.Id = cp.PendingPaymentId
LEFT OUTER JOIN dbo.Accounts a
ON a.AccountLegalEntityId = cp.AccountLegalEntityId
INNER JOIN incentives.ApprenticeshipIncentive ai
ON ai.Id = cp.ApprenticeshipIncentiveId
INNER JOIN dbo.IncentiveApplicationApprenticeship iaa
ON iaa.Id = ai.IncentiveApplicationApprenticeshipId
INNER JOIN dbo.IncentiveApplication ia
ON ia.Id = iaa.IncentiveApplicationId
INNER JOIN incentives.Payment p
ON cp.PaymentId = p.Id
INNER JOIN incentives.learner l
ON ai.Id = l.ApprenticeshipIncentiveId
LEFT OUTER JOIN incentives.ApprenticeshipDaysinLearning dil 
ON dil.learnerid = l.Id 
AND dil.CreatedDate =(SELECT MAX(CreatedDate) FROM incentives.ApprenticeshipDaysinLearning WHERE LearnerId = l.Id) 
INNER JOIN @NewPaymentGenerated npg
ON npg.ULN = ai.ULN
AND npg.EarningType = pp.EarningType
LEFT OUTER JOIN incentives.EmploymentCheck ec1
ON ec1.ApprenticeshipIncentiveId = ai.Id
AND ec1.CheckType = 'EmployedBeforeSchemeStarted'
LEFT OUTER JOIN incentives.EmploymentCheck ec2
ON ec2.ApprenticeshipIncentiveId = ai.Id
AND ec2.CheckType = 'EmployedAtStartOfApprenticeship'
LEFT OUTER JOIN incentives.EmploymentCheck ec3
ON ec3.ApprenticeshipIncentiveId = ai.Id
AND ec3.CheckType = 'EmployedAt365PaymentDueDateSecondCheck'
WHERE cp.DateClawbackSent >= @StartDate AND cp.DateClawbackSent <= @EndDate
ORDER BY
ISNULL(cp.VrfVendorId, a.VrfVendorId),
ia.SubmittedByEmail,
ai.ULN,
cp.Id