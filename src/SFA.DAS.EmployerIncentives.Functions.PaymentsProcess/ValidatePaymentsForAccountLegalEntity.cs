using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class ValidatePaymentsForAccountLegalEntity
    {
        private ILogger<ValidatePaymentsForAccountLegalEntity> _logger;

        public ValidatePaymentsForAccountLegalEntity(ILogger<ValidatePaymentsForAccountLegalEntity> logger)
        {
            _logger = logger;
        }

        [FunctionName("ValidatePaymentsForAccountLegalEntity")]
        public Task Validate([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            _logger.LogInformation($"Validating Payments for account legal entity {accountLegalEntityCollectionPeriod?.AccountLegalEntityId}, collection period {accountLegalEntityCollectionPeriod?.CollectionPeriod}");
            return Task.CompletedTask;
        }
    }
}
