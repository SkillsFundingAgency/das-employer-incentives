using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.UlnHasPayments
{
    public class UlnHasPaymentsRequest : IQuery
    {
        public long AccountLegalEntityId { get; set; }
        public long ULN { get; set; }

        public UlnHasPaymentsRequest(long accountLegalEntityId, long uln)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
        }
    }
}
