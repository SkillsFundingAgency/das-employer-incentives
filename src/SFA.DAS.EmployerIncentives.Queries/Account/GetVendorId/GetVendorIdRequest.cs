
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetVendorId
{
    public class GetVendorIdRequest : IQuery
    {
        public long LegalEntityId { get; private set; }

        public GetVendorIdRequest(long legalEntityId)
        {
            LegalEntityId = legalEntityId;
        }
    }
}
