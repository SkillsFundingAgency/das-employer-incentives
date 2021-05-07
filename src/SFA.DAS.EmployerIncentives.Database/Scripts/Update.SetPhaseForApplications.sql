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
------------------ Phase1 ----------------------------
--------------------------------------------------------
UPDATE iaa
SET iaa.Phase = 'Phase1'
FROM [dbo].[IncentiveApplicationApprenticeship] iaa INNER JOIN [dbo].[IncentiveApplication] ia
	ON iaa.IncentiveApplicationId = ia.Id
WHERE iaa.Phase = 'NotSet'
AND ia.DateSubmitted < '1 June 2021'

UPDATE ai
SET ai.Phase = 'Phase1'
FROM  [incentives].[ApprenticeshipIncentive] ai
WHERE
	ai.Phase = 'NotSet'
AND ai.SubmittedDate < '1 June 2021'


