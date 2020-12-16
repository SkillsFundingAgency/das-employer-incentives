namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.HasPayments
{
    public class HasPaymentsResponse
    {
        public bool HasPayments { get; }

        public HasPaymentsResponse(bool hasPayments)
        {
            HasPayments = hasPayments;
        }
    }
}
