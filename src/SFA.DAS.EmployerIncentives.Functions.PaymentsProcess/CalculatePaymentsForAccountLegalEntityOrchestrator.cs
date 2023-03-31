using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using Microsoft.DurableTask;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculatePaymentsForAccountLegalEntityOrchestrator
    {
        [Function(nameof(CalculatePaymentsForAccountLegalEntityOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var accountLegalEntityCollectionPeriod = context.GetInput<AccountLegalEntityCollectionPeriod>();

            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;

            var pendingPayments =
                await context.CallActivityAsync<List<PendingPaymentActivity>>(
                    nameof(GetPendingPaymentsForAccountLegalEntity), accountLegalEntityCollectionPeriod);

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