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

            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;

            var pendingPayments = await context.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            var tasks = new List<Task>();
            foreach (var pendingPayment in pendingPayments)
            {
                tasks.Add(
                    context.CallActivityAsync("ValidatePendingPayment",
                            new ValidatePendingPaymentData(
                                accountLegalEntityCollectionPeriod.CollectionPeriod.Year,
                                accountLegalEntityCollectionPeriod.CollectionPeriod.Period,
                                pendingPayment.ApprenticeshipIncentiveId,
                                pendingPayment.PendingPaymentId)));
            }

            await Task.WhenAll(tasks);

            foreach (var pendingPayment in pendingPayments)
            {
                tasks.Add(
                    context.CallActivityAsync("CreatePayment",
                        new CreatePaymentInput
                        {
                            ApprenticeshipIncentiveId = pendingPayment.ApprenticeshipIncentiveId,
                            PendingPaymentId = pendingPayment.PendingPaymentId,
                            CollectionPeriod = collectionPeriod
                        }));
            }

            await Task.WhenAll(tasks);
        }
    }
}