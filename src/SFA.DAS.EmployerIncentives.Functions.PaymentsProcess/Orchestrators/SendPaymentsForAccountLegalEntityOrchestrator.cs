using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class SendPaymentsForAccountLegalEntityOrchestrator
    {
        private readonly ILogger<SendPaymentsForAccountLegalEntityOrchestrator> _logger;

        public SendPaymentsForAccountLegalEntityOrchestrator(
            ILogger<SendPaymentsForAccountLegalEntityOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(SendPaymentsForAccountLegalEntityOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();

            if (!context.IsReplaying) 
                _logger.LogInformation("SendPaymentsForAccountLegalEntityOrchestrator started for Account Legal Entity: {accountLegalEntityCollectionPeriod}", accountLegalEntityCollectionPeriod);

            var paymentTasks = new List<Task>
            {
                context.CallActivityAsync(nameof(SendClawbacksForAccountLegalEntity), accountLegalEntityCollectionPeriod),
                context.CallActivityAsync(nameof(SendPaymentRequestsForAccountLegalEntity), accountLegalEntityCollectionPeriod)
            };

            await Task.WhenAll(paymentTasks);

            if (!context.IsReplaying) 
                _logger.LogInformation("SendPaymentsForAccountLegalEntityOrchestrator completed for Account Legal Entity: {accountLegalEntityCollectionPeriod}", accountLegalEntityCollectionPeriod);
        }
    }
}
