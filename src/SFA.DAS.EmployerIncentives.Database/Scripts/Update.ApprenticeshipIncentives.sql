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
-- EI-914 reinstate [ApprenticeshipIncentive] records for prevous withdrawals
INSERT INTO [incentives].[ApprenticeshipIncentive]
(
	[Id]
    ,[AccountId]
    ,[ApprenticeshipId]
    ,[FirstName]
    ,[LastName]
    ,[DateOfBirth]
    ,[ULN]
    ,[EmployerType]
    ,[StartDate]
    ,[IncentiveApplicationApprenticeshipId]
    ,[AccountLegalEntityId]
    ,[UKPRN]
    ,[RefreshedLearnerForEarnings]
    ,[HasPossibleChangeOfCircumstances]
    ,[PausePayments]
    ,[SubmittedDate]
    ,[SubmittedByEmail]
    ,[CourseName]
    ,[Status]
)
SELECT
	  NewID() AS "Id",
      a.AccountId,
	  aa.ApprenticeshipId,
	  aa.FirstName,
	  aa.LastName,
	  aa.DateOfBirth,
	  aa.ULN,
	  aa.ApprenticeshipEmployerTypeOnApproval AS "EmployerType",
	  aa.PlannedStartDate,
	  aa.Id AS "IncentiveApplicationApprenticeshipId",
	  a.AccountLegalEntityId,
	  aa.UKPRN,
	  0 as "RefreshedLearnerForEarnings",
	  0 as "HasPossibleChangeOfCircumstances",
	  0 as "PausePayments",
	  a.DateSubmitted,
	  a.SubmittedByEmail,
	  aa.CourseName,
	  'Withdrawn' as "Status"
FROM
	[dbo].[IncentiveApplication] a INNER JOIN [dbo].[IncentiveApplicationApprenticeship] aa
		ON a.Id = aa.IncentiveApplicationId
	LEFT OUTER JOIN [incentives].[ApprenticeshipIncentive] i
		ON i.IncentiveApplicationApprenticeshipId = aa.Id
WHERE
	(aa.WithdrawnByCompliance = 1 OR aa.WithdrawnByEmployer = 1)
	AND i.Id IS NULL
GO
