using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
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

        [Function(nameof(SendPaymentsForAccountLegalEntityOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();

            if (!context.IsReplaying) 
                _logger.LogDebug("SendPaymentsForAccountLegalEntityOrchestrator started for Account Legal Entity: {accountLegalEntityCollectionPeriod}", accountLegalEntityCollectionPeriod);

            var paymentTasks = new List<Task>
            {
                context.CallActivityAsync(nameof(SendClawbacksForAccountLegalEntity), accountLegalEntityCollectionPeriod),
                context.CallActivityAsync(nameof(SendPaymentRequestsForAccountLegalEntity), accountLegalEntityCollectionPeriod)
            };

            await Task.WhenAll(paymentTasks);

            if (!context.IsReplaying) 
                _logger.LogDebug("SendPaymentsForAccountLegalEntityOrchestrator completed for Account Legal Entity: {accountLegalEntityCollectionPeriod}", accountLegalEntityCollectionPeriod);
        }
    }
}
