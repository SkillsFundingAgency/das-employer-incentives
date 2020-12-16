using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.HasPayments
{
    public class HasPaymentsRequest : IQuery
    {
        public long AccountLegalEntityId { get; set; }
        public long ULN { get; set; }

        public HasPaymentsRequest(long accountLegalEntityId, long uln)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
        }
    }
}
