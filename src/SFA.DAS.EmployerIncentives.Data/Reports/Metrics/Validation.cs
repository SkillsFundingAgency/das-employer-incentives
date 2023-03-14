namespace SFA.DAS.EmployerIncentives.Data.Reports.Metrics
{
    public class Validation
    {
        public int Order { get; set; }
        public int CountOfPayments { get; set; }
        public bool HasLearningRecord { get; set; }
        public bool IsInLearning { get; set; }
        public bool HasDaysInLearning { get; set; }
        public bool HasNoDataLocks { get; set; }
        public bool HasBankDetails { get; set; }
        public bool PaymentsNotPaused { get; set; }
        public bool HasNoUnsentClawbacks { get; set; }
        public bool HasIlrSubmission { get; set; }
        public bool HasSignedMinVersion { get; set; }
        public bool LearnerMatchSuccessful { get; set; }
        public bool EmployedAtStartOfApprenticeship { get; set; }
        public bool EmployedBeforeSchemeStarted { get; set; }
        public bool BlockedForPayments { get; set; }
        public bool EmployedAt365Days { get; set; }
        public int NumberOfAccountLegalEntityIds { get; set; }
        public double FirstEarningAmount { get; set; }
        public double SecondEarningAmount { get; set; }
        public double TotalEarningAmount { get; set; }
    }
}
