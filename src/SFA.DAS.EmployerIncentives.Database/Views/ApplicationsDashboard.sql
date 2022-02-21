CREATE VIEW [dbo].[ApplicationsDashboard]
	AS 
	SELECT 
	ia.Id, 
	ia.DateCreated, 
	ia.[Status], 
	ia.DateSubmitted, 
	iaa.PlannedStartDate, 	
	q.TotalIncentiveAmount,
	iaa.ApprenticeshipEmployerTypeOnApproval
	FROM IncentiveApplication ia
	INNER JOIN IncentiveApplicationApprenticeship iaa
	oN ia.Id = iaa.IncentiveApplicationId
	left join	  (SELECT   ai.IncentiveApplicationApprenticeshipId,				
							sum(pp.Amount) as TotalIncentiveAmount
							FROM incentives.ApprenticeshipIncentive ai
							left join incentives.PendingPayment pp
							ON ai.Id = pp.ApprenticeshipIncentiveId
							GROUP BY ai.IncentiveApplicationApprenticeshipId
						  ) q on  q.IncentiveApplicationApprenticeshipId= iaa.Id	
	
