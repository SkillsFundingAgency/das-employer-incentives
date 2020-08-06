using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity
{
    public class GetLegalEntityRequest : IQuery
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }

        public GetLegalEntityRequest(long accountId, long accountLegalEntityId)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
