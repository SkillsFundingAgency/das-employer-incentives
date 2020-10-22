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

            var tasks = new List<Task>();
            foreach (var pendingPayment in pendingPayments)
            {
                tasks.Add(
                    context.CallActivityAsync("ValidatePendingPayment", pendingPayment)
                        .ContinueWith(previous => context.CallActivityAsync("CreatePayment",
                            new CreatePaymentInput
                            {
                                ApprenticeshipIncentiveId = pendingPayment.ApprenticeshipIncentiveId,
                                PendingPaymentId = pendingPayment.PendingPaymentId, CollectionPeriod = collectionPeriod
                            }), TaskContinuationOptions.OnlyOnRanToCompletion));
            }

            await Task.WhenAll(tasks);
        }
    }
}