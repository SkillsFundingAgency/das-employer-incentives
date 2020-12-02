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
            var failedPendingPayments = new List<ValidatePendingPaymentException>();

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

                allValidatePaymentTasks = Task.WhenAll(validatePaymentTasks);
                await allValidatePaymentTasks;
            }
            catch(Exception)
            {
                allValidatePaymentTasks?.Exception?.InnerExceptions.ToList().ForEach(e =>
                {
                    if (e.InnerException is ValidatePendingPaymentException)
                    {
                        failedPendingPayments.Add(e.InnerException as ValidatePendingPaymentException);
                    }
                });
                   
                if(!failedPendingPayments.Any())
                {
                    throw;
                }

                throw new AggregateException("Error ValidatePendingPayments", failedPendingPayments);
            }

            var createPaymentTasks = new List<Task>();
            foreach (var pendingPayment in pendingPayments)
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
            await Task.WhenAll(createPaymentTasks);
        }
    }
}