--------------------------------------------------------
-- EI-1074 Script to apply Phase flag to existing 
-- apprenticeship applications
--------------------------------------------------------

--------------------------------------------------------
--------------- Initialise to Not Set ------------------
--------------------------------------------------------
UPDATE [dbo].[IncentiveApplicationApprenticeship]
SET Phase = 'NotSet'
WHERE Phase IS NULL

UPDATE [incentives].[ApprenticeshipIncentive]
SET Phase = 'NotSet'
WHERE Phase IS NULL

--------------------------------------------------------
------------------ Phase1_0 ----------------------------
--------------------------------------------------------
UPDATE iaa
SET iaa.Phase = 'Phase1_0'
FROM [dbo].[IncentiveApplicationApprenticeship] iaa INNER JOIN [dbo].[IncentiveApplication] ia
	ON iaa.IncentiveApplicationId = ia.Id
WHERE iaa.Phase = 'NotSet'
AND ia.DateSubmitted < '1 June 2021'
AND iaa.PlannedStartDate >= '1 August 2020'
AND iaa.PlannedStartDate < '1 February 2021'

UPDATE ai
SET ai.Phase = 'Phase1_0'
FROM  [incentives].[ApprenticeshipIncentive] ai LEFT OUTER JOIN [incentives].[Learner] l
	ON ai.Id = l.ApprenticeshipIncentiveId
WHERE
	ai.Phase = 'NotSet'
AND l.StartDate IS NULL
AND ai.SubmittedDate < '1 June 2021'
AND ai.StartDate  >= '1 August 2020'
AND ai.StartDate < '1 February 2021'

UPDATE ai
SET ai.Phase = 'Phase1_0'
FROM  [incentives].[ApprenticeshipIncentive] ai LEFT OUTER JOIN [incentives].[Learner] l
	ON ai.Id = l.ApprenticeshipIncentiveId
WHERE
	ai.Phase = 'NotSet'
AND l.StartDate IS NOT NULL
AND ai.SubmittedDate < '1 June 2021'
AND l.StartDate   >= '1 August 2020'
AND l.StartDate < '1 February 2021'

--------------------------------------------------------
------------------ Phase1_1 ----------------------------
--------------------------------------------------------
UPDATE iaa
SET iaa.Phase = 'Phase1_1'
FROM [dbo].[IncentiveApplicationApprenticeship] iaa INNER JOIN [dbo].[IncentiveApplication] ia
	ON iaa.IncentiveApplicationId = ia.Id
WHERE iaa.Phase = 'NotSet'
AND ia.DateSubmitted < '1 June 2021'
AND iaa.PlannedStartDate >= '1 February 2021'
AND iaa.PlannedStartDate < '1 April 2021'

UPDATE ai
SET ai.Phase = 'Phase1_1'
FROM  [incentives].[ApprenticeshipIncentive] ai LEFT OUTER JOIN [incentives].[Learner] l
	ON ai.Id = l.ApprenticeshipIncentiveId
WHERE
	ai.Phase = 'NotSet'
AND l.StartDate IS NULL
AND ai.SubmittedDate < '1 June 2021'
AND ai.StartDate  >= '1 February 2021'
AND ai.StartDate < '1 April 2021'

UPDATE ai
SET ai.Phase = 'Phase1_1'
FROM  [incentives].[ApprenticeshipIncentive] ai LEFT OUTER JOIN [incentives].[Learner] l
	ON ai.Id = l.ApprenticeshipIncentiveId
WHERE
	ai.Phase = 'NotSet'
AND l.StartDate IS NOT NULL
AND ai.SubmittedDate < '1 June 2021'
AND l.StartDate   >= '1 February 2021'
AND l.StartDate < '1 June 2021'