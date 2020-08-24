CREATE VIEW [dbo].[ApplicationsDashboard]
	AS 
	SELECT 
	ia.Id, 
	ia.DateCreated, 
	ia.[Status], 
	ia.DateSubmitted, 
	iaa.PlannedStartDate, 
	iaa.TotalIncentiveAmount,
	iaa.ApprenticeshipEmployerTypeOnApproval
	FROM IncentiveApplication ia
	INNER JOIN IncentiveApplicationApprenticeship iaa
	oN ia.Id = iaa.IncentiveApplicationId
