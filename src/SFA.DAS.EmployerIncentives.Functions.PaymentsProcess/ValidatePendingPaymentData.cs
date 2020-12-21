namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class ValidatePendingPaymentData
    {
        public ValidatePendingPaymentData(short year, byte period, System.Guid apprenticeshipIncentiveId, System.Guid pendingPaymentId)
        {
            Year = year;
            Period = period;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
        }

        public short Year { get; }
        public byte Period { get; }
        public System.Guid ApprenticeshipIncentiveId { get; }
        public System.Guid PendingPaymentId { get; }
    }
}