
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetVendorId
{
    public class GetVendorIdRequest : IQuery
    {
        public string HashedLegalEntityId { get; private set; }

        public GetVendorIdRequest(string hashedLegalEntityId)
        {
            HashedLegalEntityId = hashedLegalEntityId;
        }
    }
}
