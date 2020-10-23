using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculatePaymentsForAccountLegalEntityOrchestrator
    {

        [FunctionName("CalculatePaymentsForAccountLegalEntityOrchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();

            var pendingPayments = await context.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            var validationTasks = new List<Task>();

            foreach (var pendingPayment in pendingPayments)
            {
                validationTasks.Add(context.CallActivityAsync("ValidatePendingPayment", new ValidatePendingPaymentData(accountLegalEntityCollectionPeriod.CollectionPeriod.Year,
                        accountLegalEntityCollectionPeriod.CollectionPeriod.Month, pendingPayment.ApprenticeshipIncentiveId, pendingPayment.PendingPaymentId)));
            }

            await Task.WhenAll(validationTasks);
        }
    }
}