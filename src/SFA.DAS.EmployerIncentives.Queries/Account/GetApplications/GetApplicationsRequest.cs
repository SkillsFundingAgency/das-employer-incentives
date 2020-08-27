using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsRequest : IQuery
    {
        public long AccountId { get; }

        public GetApplicationsRequest(long accountId)
        {
            AccountId = accountId;
        }
    }
}
