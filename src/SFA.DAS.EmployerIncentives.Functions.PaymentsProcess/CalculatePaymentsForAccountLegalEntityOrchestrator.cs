using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculatePaymentsForAccountLegalEntityOrchestrator
    {
        private readonly ILogger<CalculatePaymentsForAccountLegalEntityOrchestrator> _logger;

        public CalculatePaymentsForAccountLegalEntityOrchestrator(ILogger<CalculatePaymentsForAccountLegalEntityOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName("CalculatePaymentsForAccountLegalEntityOrchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;

            var pendingPayments = await context.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            var validationTasks = new List<Task>();

            foreach (var pendingPayment in pendingPayments)
            {
                validationTasks.Add(context.CallActivityAsync("ValidatePendingPayment", new ValidatePendingPaymentData(accountLegalEntityCollectionPeriod.CollectionPeriod.Year,
                        accountLegalEntityCollectionPeriod.CollectionPeriod.Month, pendingPayment.ApprenticeshipIncentiveId, pendingPayment.PendingPaymentId)));
                _logger.LogInformation($"Request made to validate pending payment for pending payment id {pendingPayment.PendingPaymentId}", new { accountLegalEntityId, collectionPeriod, pendingPayment = pendingPayment.PendingPaymentId });
            }

            await Task.WhenAll(validationTasks);
        }
    }
}