using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculatePaymentsForAccountLegalEntityOrchestrator
    {
        [FunctionName(nameof(CalculatePaymentsForAccountLegalEntityOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();

            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;

            var pendingPayments = await context.CallActivityAsync<List<PendingPaymentActivityDto>>(nameof(GetPendingPaymentsForAccountLegalEntity), accountLegalEntityCollectionPeriod);

            var tasks = new List<Task>();
            foreach (var pendingPayment in pendingPayments)
            {
                tasks.Add(
                    context.CallActivityAsync(nameof(ValidatePendingPayment),
                            new ValidatePendingPaymentData(
                                accountLegalEntityCollectionPeriod.CollectionPeriod.Year,
                                accountLegalEntityCollectionPeriod.CollectionPeriod.Period,
                                pendingPayment.ApprenticeshipIncentiveId,
                                pendingPayment.PendingPaymentId))
                        .ContinueWith(previous => context.CallActivityAsync(nameof(CreatePayment),
                            new CreatePaymentInput
                            {
                                ApprenticeshipIncentiveId = pendingPayment.ApprenticeshipIncentiveId,
                                PendingPaymentId = pendingPayment.PendingPaymentId,
                                CollectionPeriod = collectionPeriod
                            }), TaskContinuationOptions.OnlyOnRanToCompletion));
            }

            await Task.WhenAll(tasks);
        }
    }
}