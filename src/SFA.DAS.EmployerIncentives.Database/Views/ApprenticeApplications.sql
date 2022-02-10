CREATE VIEW [incentives].[ApprenticeApplications]
	AS 
SELECT
DISTINCT ai.Id,
ai.AccountId,
ai.AccountLegalEntityId,
ai.[Status],
ISNULL(ai.SubmittedDate, GETDATE()) AS SubmittedDate,
ai.FirstName,
ai.LastName,
ai.ULN,
ai.MinimumAgreementVersion,
a.LegalEntityName,
a.SignedAgreementVersion,
ai.SubmittedByEmail,
ai.CourseName,
ai.PausePayments,
ai.WithdrawnBy,
pp1.Amount AS [FirstPendingPaymentAmount],
pp1.DueDate AS [FirstPendingPaymentDueDate],
pp2.Amount AS [SecondPendingPaymentAmount],
pp2.DueDate AS [SecondPendingPaymentDueDate],
p1.Amount AS [FirstPaymentAmount],
p1.PaidDate AS [FirstPaymentDate],
p1.CalculatedDate AS [FirstPaymentCalculatedDate],
p2.Amount AS [SecondPaymentAmount],
p2.PaidDate AS [SecondPaymentDate],
p2.CalculatedDate AS [SecondPaymentCalculatedDate],
c1.Amount AS [FirstClawbackAmount],
c1.DateClawbackCreated AS [FirstClawbackCreated],
c1.DateClawbackSent AS [FirstClawbackSent],
c2.Amount AS [SecondClawbackAmount],
c2.DateClawbackCreated AS [SecondClawbackCreated],
c2.DateClawbackSent AS [SecondClawbackSent],
l.LearningFound,
l.HasDataLock,
l.InLearning,
ppvr1.Result AS FirstEmploymentCheckValidation,
ec1.Result AS FirstEmploymentCheckResult,
ppvr2.Result AS SecondEmploymentCheckValidation,
ec2.Result AS SecondEmploymentCheckResult
FROM incentives.ApprenticeshipIncentive ai
INNER JOIN dbo.Accounts a
ON ai.AccountLegalEntityId = a.AccountLegalEntityId
LEFT OUTER JOIN incentives.PendingPayment pp1
ON pp1.ApprenticeshipIncentiveId = ai.Id
AND pp1.EarningType = 'FirstPayment'
AND pp1.ClawedBack = 0
LEFT OUTER JOIN incentives.PendingPayment ppc1
ON ppc1.ApprenticeshipIncentiveId = ai.Id
AND ppc1.EarningType = 'FirstPayment'
AND ppc1.ClawedBack = 1
LEFT OUTER JOIN incentives.PendingPayment pp2
ON pp2.ApprenticeshipIncentiveId = ai.Id
AND pp2.EarningType = 'SecondPayment'
AND pp2.ClawedBack = 0
LEFT OUTER JOIN incentives.PendingPayment ppc2
ON ppc2.ApprenticeshipIncentiveId = ai.Id
AND ppc2.EarningType = 'SecondPayment'
AND ppc2.ClawedBack = 1
LEFT OUTER JOIN incentives.Payment p1
ON p1.PendingPaymentId = pp1.Id
LEFT OUTER JOIN incentives.Payment p2
ON p2.PendingPaymentId = pp2.Id
LEFT OUTER JOIN incentives.ClawbackPayment c1
ON c1.PaymentId = p1.Id
LEFT OUTER JOIN incentives.ClawbackPayment c2
ON c2.PaymentId = p2.Id
LEFT OUTER JOIN incentives.Learner l
ON l.ApprenticeshipIncentiveId = ai.Id
LEFT OUTER JOIN incentives.PendingPaymentValidationResult ppvr1
ON ppvr1.PendingPaymentId = pp1.Id
AND ppvr1.Step = 'EmployedAtStartOfApprenticeship'
LEFT OUTER JOIN incentives.PendingPaymentValidationResult ppvr2
ON ppvr2.PendingPaymentId = pp1.Id
AND ppvr2.Step = 'EmployedBeforeSchemeStarted'
LEFT OUTER JOIN incentives.EmploymentCheck ec1
ON ec1.ApprenticeshipIncentiveId = ai.Id
AND ec1.CheckType = 'EmployedAtStartOfApprenticeship'
LEFT OUTER JOIN incentives.EmploymentCheck ec2
ON ec2.ApprenticeshipIncentiveId = ai.Id
AND ec2.CheckType = 'EmployedBeforeSchemeStarted'


