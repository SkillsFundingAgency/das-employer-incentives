using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity
{
    public class GetApprenticeshipIncentivesForAccountLegalEntityRequest : IQuery
    {
        public long AccountId { get; }

        public long AccountLegalEntityId { get; }

        public GetApprenticeshipIncentivesForAccountLegalEntityRequest(long accountId, long accountLegalEntityId)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
