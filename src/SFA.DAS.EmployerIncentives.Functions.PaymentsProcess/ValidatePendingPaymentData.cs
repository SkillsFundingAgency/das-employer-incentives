namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class ValidatePendingPaymentData
    {
        public ValidatePendingPaymentData(short year, byte month, System.Guid apprenticeshipIncentiveId, System.Guid pendingPaymentId)
        {
            Year = year;
            Month = month;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
        }

        public short Year { get; }
        public byte Month { get; }
        public System.Guid ApprenticeshipIncentiveId { get; }
        public System.Guid PendingPaymentId { get; }
    }
}