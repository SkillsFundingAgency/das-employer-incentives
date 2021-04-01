UPDATE [incentives].ApprenticeshipIncentive
SET MinimumAgreementVersion = 4
WHERE Id IN (
  SELECT ai.Id FROM
  [incentives].[ApprenticeshipIncentive] ai
  INNER JOIN incentives.Learner l
    on ai.Id = l.ApprenticeshipIncentiveId  
    AND l.StartDate >= '2020-08-01'
    AND l.StartDate < '2021-02-01' 
    AND l.LearningFound = 1
)
AND MinimumAgreementVersion IS NULL
  
UPDATE [incentives].ApprenticeshipIncentive
SET MinimumAgreementVersion = 5
WHERE Id IN (
  SELECT ai.Id FROM
    [incentives].[ApprenticeshipIncentive] ai
  INNER JOIN incentives.Learner l
    ON ai.Id = l.ApprenticeshipIncentiveId  
    AND l.StartDate >= '2021-02-01'
    AND l.StartDate <= '2021-03-31'
    AND l.LearningFound = 1
)
AND MinimumAgreementVersion IS NULL

UPDATE [incentives].ApprenticeshipIncentive
SET MinimumAgreementVersion = 4
WHERE Id IN (
  SELECT ai.Id FROM
    [incentives].[ApprenticeshipIncentive] ai
    LEFT OUTER JOIN incentives.Learner l
    ON ai.Id = l.ApprenticeshipIncentiveId  
    AND l.LearningFound = 0
)  
AND StartDate >= '2020-08-01'
AND StartDate < '2021-02-01' 
AND MinimumAgreementVersion IS NULL
  
UPDATE [incentives].ApprenticeshipIncentive
SET MinimumAgreementVersion = 5
WHERE Id IN (
  SELECT ai.Id FROM
    [incentives].[ApprenticeshipIncentive] ai
    LEFT OUTER JOIN incentives.Learner l
    ON ai.Id = l.ApprenticeshipIncentiveId  
    AND l.LearningFound = 0
  )  
AND StartDate >= '2021-02-01' 
AND StartDate <= '2021-03-31'
AND MinimumAgreementVersion IS NULL
