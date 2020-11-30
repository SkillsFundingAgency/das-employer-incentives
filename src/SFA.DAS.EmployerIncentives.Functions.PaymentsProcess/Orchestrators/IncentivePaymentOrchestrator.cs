using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class IncentivePaymentOrchestrator
    {
        private ILogger<IncentivePaymentOrchestrator> _logger;

        public IncentivePaymentOrchestrator(ILogger<IncentivePaymentOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName("IncentivePaymentOrchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var collectionPeriod = context.GetInput<CollectionPeriod>();

            if(!context.IsReplaying)
                _logger.LogInformation("Incentive Payment process started for collection period {collectionPeriod}", collectionPeriod);

            context.SetCustomStatus("GettingPayableLegalEntities");
            var payableLegalEntities = await context.CallActivityAsync<List<PayableLegalEntityDto>>("GetPayableLegalEntities", collectionPeriod);

            context.SetCustomStatus("CalculatingPayments");
            var calculatePaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                var calculatePaymentTask = context.CallSubOrchestratorAsync("CalculatePaymentsForAccountLegalEntityOrchestrator", new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                calculatePaymentTasks.Add(calculatePaymentTask);
            }

            await Task.WhenAll(calculatePaymentTasks);

            context.SetCustomStatus("WaitingForPaymentApproval");

            var paymentsApproved = await context.WaitForExternalEvent<bool>("PaymentsApproved");
            if (!paymentsApproved)
            {
                context.SetCustomStatus("PaymentsRejected");
                _logger.LogInformation("Calculated payments for collection period {collectionPeriod} have been rejected", collectionPeriod);
                return;
            }

            context.SetCustomStatus("SendingPayments");
            _logger.LogInformation("Calculated payments for collection period {collectionPeriod} have been approved", collectionPeriod);

            var sendPaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                var sendPaymentTask = context.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                sendPaymentTasks.Add(sendPaymentTask);
            }
            await Task.WhenAll(sendPaymentTasks);
            context.SetCustomStatus("PaymentsSent");

            _logger.LogInformation("Incentive Payment process completed for collection period {collectionPeriod}", collectionPeriod);
        }
    }
}