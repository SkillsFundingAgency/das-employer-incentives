using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetVendorIdForAccountLegalEntity
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private ILogger<GetVendorIdForAccountLegalEntity> _logger;

        public GetVendorIdForAccountLegalEntity(IQueryDispatcher queryDispatcher, ILogger<GetVendorIdForAccountLegalEntity> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName("GetVendorIdForAccountLegalEntity")]
        public async Task<string> Get([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            _logger.LogInformation($"Getting VendorId for account legal entity {accountLegalEntityCollectionPeriod.AccountLegalEntityId}, collection period {accountLegalEntityCollectionPeriod.AccountId}");
            var legalEntity = await _queryDispatcher.Send<GetLegalEntityRequest, GetLegalEntityResponse>(new GetLegalEntityRequest(accountLegalEntityCollectionPeriod.AccountId, accountLegalEntityCollectionPeriod.AccountLegalEntityId));

            return legalEntity?.LegalEntity?.VrfVendorId;
        }
    }
}
