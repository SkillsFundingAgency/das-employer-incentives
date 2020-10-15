using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class ValidatePaymentsForAccountLegalEntity
    {
        private readonly ICommandDispatcher _queryDispatcher;
        private ILogger<ValidatePaymentsForAccountLegalEntity> _logger;

        public ValidatePaymentsForAccountLegalEntity( //ICommandDispatcher queryDispatcher,
                                                      ILogger<ValidatePaymentsForAccountLegalEntity> logger)
        {
            //_queryDispatcher = queryDispatcher;
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
