/*
This query is used by support and shows learners for the given vendor. There will be two rows for each ULN one to represent each payment.
NOTE: VRFVendorID has a one to one relationship with Legal entity in the Apprenticeship Service.

VendorId format: P0002196
ULN format: 1057862088
HashedLegalEntityId: 8MGNNW

Usage: Enter either a VendorId or ULN. The procedure will recognise a VendorId or teat it as a ULN
*/
CREATE procedure [support].[PaymentEnquiry]
(
	@vendorid varchar(100)='',
	@uln varchar(20)='', 
	@hashedlegalentityid varchar(8)=''
)
AS
	select  ULN,
			[SubmissionFound], 
			[LearningFound], 
			[HasDataLock], 
			[InLearning], 
			q.HasBank, 
			PausedPayments, 
			[DaysInLearning], 
			NumberOfDaysInLearning,
			lp.FirstLearning,
			convert(datetime,l.UpdatedDate,101) as [ILRDataUpdated],
			sum(pp.Amount) as [EarningAmount], 
			convert(nvarchar(10),pp.DueDate,126) as EarningDueDate, 
			pp.PeriodNumber as [EarningPeriod],
			pp.PaymentYear as [EarningYear], 
			p.PaymentPeriod,
			p.PaymentYear, 
			p.PaidDate,
			a.VrfVendorId,
			HashedLegalEntityId
	from [incentives].[Learner] l
	left join (select Id, AccountLegalEntityId, case when PausePayments = 1 then 1 else 0 end as PausedPayments from [incentives].[ApprenticeshipIncentive]) ai on ai.Id = l.ApprenticeshipIncentiveId
	left join (select AccountLegalEntityId, case when VrfVendorId is not null then 1 else 0 end as HasBank from [dbo].[Accounts] a) q on ai.AccountLegalEntityId = q.AccountLegalEntityId
	left join [incentives].[PendingPayment] pp on pp.ApprenticeshipIncentiveId = l.ApprenticeshipIncentiveId
	left join (select LearnerId, case when NumberOfDaysInLearning >= 90 then 1 else 0 end as [DaysInLearning], NumberOfDaysInLearning from [incentives].[ApprenticeshipDaysInLearning]) adil on adil.LearnerId=l.Id
	left join [dbo].[Accounts] a on a.AccountLegalEntityId=pp.AccountLegalEntityId
	left join [incentives].[Payment] p on p.PendingPaymentId=pp.Id
	left join (select LearnerId,min(StartDate) as FirstLearning from [incentives].[LearningPeriod] group by LearnerId) lp on lp.LearnerId=l.Id
	where 1=1
	and (a.VrfVendorId=@vendorid or ULN=@uln or HashedLegalEntityId=@hashedlegalentityid)
	group by ULN,
			[SubmissionFound], 
			[LearningFound], 
			[HasDataLock], 
			[InLearning], 
			q.HasBank, 
			PausedPayments, 
			DaysInLearning,
			NumberOfDaysInLearning, 
			lp.FirstLearning,
			l.UpdatedDate, 
			pp.DueDate,
			pp.PeriodNumber,
			pp.PaymentYear, 
			p.PaymentPeriod,
			p.PaymentYear, 
			p.PaidDate, 
			a.VrfVendorId, 
			HashedLegalEntityId
	order by ULN, pp.PaymentYear,pp.PeriodNumber

Return @@rowcount