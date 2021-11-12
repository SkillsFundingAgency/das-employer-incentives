using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class SendClawbacksForAccountLegalEntityOrchestrator
    {
        private readonly ILogger<SendClawbacksForAccountLegalEntityOrchestrator> _logger;

        public SendClawbacksForAccountLegalEntityOrchestrator(
            ILogger<SendClawbacksForAccountLegalEntityOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(SendClawbacksForAccountLegalEntityOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();
            
            if (!context.IsReplaying)
                _logger.LogInformation("SendClawbacksForAccountLegalEntityOrchestrator started for Account Legal Entity: {accountLegalEntityCollectionPeriod}", accountLegalEntityCollectionPeriod);

            await context.CallActivityAsync(nameof(SendClawbacksForAccountLegalEntity), accountLegalEntityCollectionPeriod);

            if (!context.IsReplaying)
                _logger.LogInformation("SendClawbacksForAccountLegalEntityOrchestrator completed for Account Legal Entity: {accountLegalEntityCollectionPeriod}", accountLegalEntityCollectionPeriod);
        }
    }
}
