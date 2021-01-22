CREATE VIEW [dbo].[BusinessValidationRulesDashboard]
as
select count(*) as [Learners], [SubmissionFound], [LearningFound], [HasDataLock], [InLearning], q.HasBank, PausedPayments, [DaysInLearning],sum(pp.amount) as [EarningAmount], count(distinct VrfVendorId) as NumVendors
  from [incentives].[Learner] l
  left join (select id, AccountLegalEntityId, case when PausePayments = 1 then 1 else 0 end as PausedPayments from [incentives].[ApprenticeshipIncentive]) ai on ai.Id = l.ApprenticeshipIncentiveId
  left join (select AccountLegalEntityId, case when vrfvendorid is not null then 1 else 0 end as HasBank from [dbo].[Accounts] a) q on ai.AccountLegalEntityId = q.AccountLegalEntityId
  left join [incentives].[PendingPayment] pp on pp.ApprenticeshipIncentiveId = l.ApprenticeshipIncentiveId
  left join (select learnerid, case when NumberOfDaysInLearning >= 89 then 1 else 0 end as [DaysInLearning] from [incentives].[ApprenticeshipDaysInLearning]) adil on adil.LearnerId=l.Id
  left join [dbo].[Accounts] a on a.AccountLegalEntityId=pp.AccountLegalEntityId
  where a.VrfCaseStatusLastUpdatedDateTime <= pp.CalculatedDate
  group by [SubmissionFound], [LearningFound], [HasDataLock], [InLearning], q.HasBank, PausedPayments, DaysInLearning

