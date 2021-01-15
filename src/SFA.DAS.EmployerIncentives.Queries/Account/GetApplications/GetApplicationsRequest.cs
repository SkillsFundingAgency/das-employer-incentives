using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsRequest : IQuery
    {
        public long AccountId { get; }

        public long AccountLegalEntityId { get; }
        public GetApplicationsRequest(long accountId, long accountLegalEntityId)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
