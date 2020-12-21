using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity
{
    public class GetLegalEntityByHashedIdRequest : IQuery
    {
        public string HashedLegalEntityId { get; private set; }

        public GetLegalEntityByHashedIdRequest(string hashedLegalEntityId)
        {
            HashedLegalEntityId = hashedLegalEntityId;
        }
    }
}
