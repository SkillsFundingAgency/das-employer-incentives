using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class ValidatePendingPayment
    {
        private ILogger<ValidatePendingPayment> _logger;

        public ValidatePendingPayment(IQueryDispatcher queryDispatcher, ILogger<ValidatePendingPayment> logger)
        {
            _logger = logger;
        }

        [FunctionName("ValidatePendingPayment")]
        public async Task Validate([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            _logger.LogInformation($"Validating Pending Payment {accountLegalEntityCollectionPeriod.AccountLegalEntityId}, collection period {accountLegalEntityCollectionPeriod.AccountId}");
        }
    }
}
