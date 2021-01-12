namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.UlnHasPayments
{
    public class UlnHasPaymentsResponse
    {
        public bool HasPayments { get; }

        public UlnHasPaymentsResponse(bool hasPayments)
        {
            HasPayments = hasPayments;
        }
    }
}
