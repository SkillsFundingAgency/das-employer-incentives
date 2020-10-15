using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculatePaymentsForAccountLegalEntityOrchestrator
    {
        private ILogger<CalculatePaymentsForAccountLegalEntityOrchestrator> _logger;

        public CalculatePaymentsForAccountLegalEntityOrchestrator(ILogger<CalculatePaymentsForAccountLegalEntityOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName("CalculatePaymentsForAccountLegalEntityOrchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;

            await _logger.LogInformationAsync($"Calculate Payments process started for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", new { accountLegalEntityId, collectionPeriod  });

            var pendingPayments = await context.CallActivityAsync<List<Guid>>("GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            foreach (var pendingPaymentId in pendingPayments)
            {
                await _logger.LogInformationAsync($"Request made to validate pending payment for pending payment id {pendingPaymentId}", new { accountLegalEntityId, collectionPeriod, pendingPayment = pendingPaymentId });
            }

            await _logger.LogInformationAsync($"Calculate Payments process completed for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", new { accountLegalEntityId, collectionPeriod });
        }
    }
}