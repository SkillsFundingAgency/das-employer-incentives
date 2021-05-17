UPDATE incentives.ApprenticeshipIncentive SET 
	UKPRN = IAA.UKPRN
FROM incentives.ApprenticeshipIncentive AI
INNER JOIN IncentiveApplicationApprenticeship IAA ON IAA.Id = AI.IncentiveApplicationApprenticeshipId
WHERE AI.UKPRN IS NULL
GO
UPDATE i SET 
	i.Status = 'Active'
FROM incentives.ApprenticeshipIncentive i
WHERE i.Status IS NULL
AND  i.PausePayments = 0
GO
UPDATE i SET 
	i.Status = 'Paused'
FROM incentives.ApprenticeshipIncentive i
WHERE i.Status IS NULL
AND  i.PausePayments = 1
GO
ALTER TABLE [archive].[PendingPayment] DROP CONSTRAINT IF EXISTS [FK_PendingPaymentArchive_ApprenticeshipIncentive]
GO