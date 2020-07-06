using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Tables;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public static class RequestExtensions
    {
        public static AddLegalEntityRequest ToAddLegalEntityRequest(this AccountTable account)
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
