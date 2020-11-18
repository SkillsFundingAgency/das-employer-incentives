UPDATE incentives.ApprenticeshipIncentive SET 
	UKPRN = IAA.UKPRN
FROM incentives.ApprenticeshipIncentive AI
INNER JOIN IncentiveApplicationApprenticeship IAA ON IAA.Id = AI.IncentiveApplicationApprenticeshipId
WHERE AI.UKPRN IS NULL
