using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var pendingPayments =
                await context.CallActivityAsync<List<PendingPaymentActivityDto>>(
                    "GetPendingPaymentsForAccountLegalEntity", accountLegalEntityCollectionPeriod);

            Task allValidatePaymentTasks = null;
            var failedPendingPaymentIds = new List<Guid>();            

            try
            {
                var validatePaymentTasks = new List<Task>();

                foreach (var pendingPayment in pendingPayments)
                {
                    validatePaymentTasks.Add(
                        context.CallActivityAsync(nameof(ValidatePendingPayment),
                            new ValidatePendingPaymentData(
                                accountLegalEntityCollectionPeriod.CollectionPeriod.Year,
                                accountLegalEntityCollectionPeriod.CollectionPeriod.Period,
                                pendingPayment.ApprenticeshipIncentiveId,
                                pendingPayment.PendingPaymentId)));
                }

                allValidatePaymentTasks = Task.WhenAll(validatePaymentTasks );

                await allValidatePaymentTasks;
            }
            catch
            {
                if (allValidatePaymentTasks?.Exception?.InnerExceptions != null && allValidatePaymentTasks.Exception.InnerExceptions.Any())
                {
                    foreach (var inner in allValidatePaymentTasks.Exception.InnerExceptions)
                    {
                        if (inner.InnerException is ValidatePendingPaymentException)
                        {
                            failedPendingPaymentIds.Add((inner.InnerException as ValidatePendingPaymentException).PendingPaymentId);
                        }
                    }
                }
            }

            var createPaymentTasks = new List<Task>();
            foreach (var pendingPayment in pendingPayments)
            {
                if (!failedPendingPaymentIds.Contains(pendingPayment.PendingPaymentId))
                {
                    createPaymentTasks.Add(
                    context.CallActivityAsync(nameof(CreatePayment),
                        new CreatePaymentInput
                        {
                            ApprenticeshipIncentiveId = pendingPayment.ApprenticeshipIncentiveId,
                            PendingPaymentId = pendingPayment.PendingPaymentId,
                            CollectionPeriod = collectionPeriod
                        }));
                }
            }

            await Task.WhenAll(createPaymentTasks);            
        }
    }
}