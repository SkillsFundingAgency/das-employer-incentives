using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities
{
    public class GetLegalEntitiesRequest : IQuery
    {
        public long AccountId { get; }

        public GetLegalEntitiesRequest(long accountId)
        {
            AccountId = accountId;
        }
    }
}
