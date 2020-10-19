using System;
using System.Collections.Generic;
using System.Linq;
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

            //await context.CallActivityAsync("ValidatePaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            var pendingPayments = await context.CallActivityAsync<List<Guid>>("GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);


            foreach (var pendingPaymentId in pendingPayments)
            {
                //TODO: this logging should be removed when an activity is called from here but it in place to allow testing in the short term.
                _logger.LogInformation($"Request made to validate pending payment for pending payment id {pendingPaymentId}", new { accountLegalEntityId, collectionPeriod, pendingPayment = pendingPaymentId });
            }
        }
    }
}