CREATE VIEW [dbo].[BusinessValidationRulesByPeriodDashboard]
	AS

with PendingPaymentValidations as (
	select	PendingPaymentId, 
			max(iif(Step='HasIlrSubmission',1,0)) as HasIlrSubmission,
			max(iif(Step='HasLearningRecord',1,0)) as HasLearningRecord,
			max(iif(Step='IsInLearning',1,0)) as IsInLearning,
			max(iif(Step='HasDaysInLearning',1,0)) as HasDaysInLearning,
			max(iif(Step='HasNoDataLocks',1,0)) as HasNoDataLocks,
			max(iif(Step='HasBankDetails',1,0)) as HasBankDetails,
			max(iif(Step='PaymentsNotPaused',1,0)) as PaymentsNotPaused,
			Result,
			PeriodNumber, 
			PaymentYear
	from	[incentives].[PendingPaymentValidationResult] ppvr
	where	ppvr.PeriodNumber = (select max(PeriodNumber) from [BusinessGetMonthEndRuntimes])
	and		ppvr.PaymentYear = (select max(PaymentYear) from [BusinessGetMonthEndRuntimes])
	group by PendingPaymentId, result, PeriodNumber, PaymentYear
)
select count(distinct pendingpaymentId) as CountOfPayments, 
		HasLearningRecord, 
		IsInLearning, 
		HasDaysInLearning, 
		HasNoDataLocks, 
		HasBankDetails, 
		PaymentsNotPaused
from PendingPaymentValidations
group by HasIlrSubmission, 
		HasLearningRecord, 
		IsInLearning, 
		HasDaysInLearning, 
		HasNoDataLocks, 
		HasBankDetails, 
		PaymentsNotPaused, 
		Result


