using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

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

            var vendorId = await context.CallActivityAsync<string>("GetVendorIdForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            var pendingPayments = await context.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            foreach (var pendingPayment in pendingPayments)
            {
                await context.CallActivityAsync<string>("ValidatePendingPayment", pendingPayment);
                //TODO: this logging should be removed when an activity is called from here but it in place to allow testing in the short term.
                _logger.LogInformation($"Request made to validate pending payment for pending payment id {pendingPayment.PendingPaymentId}", new { accountLegalEntityId, collectionPeriod, pendingPayment = pendingPayment.PendingPaymentId });
            }
        }
    }
}