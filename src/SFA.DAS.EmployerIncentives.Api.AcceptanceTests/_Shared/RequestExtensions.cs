using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public static class RequestExtensions
    {
        public static AddLegalEntityRequest ToAddLegalEntityRequest(this Account account)
        {
            return new AddLegalEntityRequest
            {
                AccountLegalEntityId = account.AccountLegalEntityId,
                LegalEntityId = account.LegalEntityId,
                OrganisationName = account.LegalEntityName
            };
        }
    }
}
