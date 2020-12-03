CREATE VIEW [dbo].[BusinessApplicationsDashboard]
	AS 
		select
			month(PlannedStartDate) as [Planned Start Month],
			year(PlannedStartDate) as [Planned Start Year],
			count(distinct(iaa.IncentiveApplicationId)) as Applications, 
			avg(cast(Learners as float)) as [Mean learners per app],
			count(Learners) as [Num Learners],
			sum(iaa.TotalIncentiveAmount) as [Total Value], 
			sum(case when iaa.TotalIncentiveAmount < '2000.00' then iaa.TotalIncentiveAmount else 0 end) as [<£2000],
			sum(case when iaa.TotalIncentiveAmount >= '2000.00' then iaa.TotalIncentiveAmount else 0 end) as [>=£2000]
		from
			[dbo].[IncentiveApplicationApprenticeship] iaa
			left join [dbo].[IncentiveApplication] ia on ia.Id=iaa.IncentiveApplicationId
			left join	  (SELECT 
							[IncentiveApplicationId],
							count(*) as Learners,
							status
							FROM [dbo].[IncentiveApplicationApprenticeship] iaa2
							left join [dbo].[IncentiveApplication] ia2 on ia2.Id=iaa2.IncentiveApplicationId
							group by [IncentiveApplicationId], status
						  ) q on  q.IncentiveApplicationId = iaa.IncentiveApplicationId
		where
			q.status = 'Submitted'
		group by 
			month(PlannedStartDate), year(PlannedStartDate)