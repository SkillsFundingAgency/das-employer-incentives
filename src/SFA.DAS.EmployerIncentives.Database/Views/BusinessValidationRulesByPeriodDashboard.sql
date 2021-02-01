CREATE VIEW [dbo].[BusinessValidationRulesByPeriodDashboard]
	AS

with PendingPaymentValidations as (
	select	PendingPaymentId, 
			iif(step='HasIlrSubmission',1,0) as HasIlrSubmission,
			iif(step='HasLearningRecord',1,0) as HasLearningRecord,
			iif(step='IsInLearning',1,0) as IsInLearning,
			iif(step='HasDaysInLearning',1,0) as HasDaysInLearning,
			iif(step='HasNoDataLocks',1,0) as HasNoDataLocks,
			iif(step='HasBankDetails',1,0) as HasBankDetails,
			iif(step='PaymentsNotPaused',1,0) as PaymentsNotPaused,
			result,
			PeriodNumber, 
			PaymentYear
	from	[incentives].[PendingPaymentValidationResult] ppvr
	where	ppvr.PeriodNumber = (select max(PeriodNumber) from [BusinessGetMonthEndRuntimes])
	and		ppvr.PaymentYear = (select max(PaymentYear) from [BusinessGetMonthEndRuntimes])
	group by PendingPaymentId, step, result, PeriodNumber, PaymentYear
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


