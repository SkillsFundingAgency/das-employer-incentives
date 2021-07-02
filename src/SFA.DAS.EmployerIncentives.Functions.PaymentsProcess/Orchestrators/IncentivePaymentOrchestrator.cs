using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class IncentivePaymentOrchestrator
    {
        private readonly ILogger<IncentivePaymentOrchestrator> _logger;

        public IncentivePaymentOrchestrator(ILogger<IncentivePaymentOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(IncentivePaymentOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var collectionPeriod = await context.CallActivityAsync<CollectionPeriod>(nameof(GetActiveCollectionPeriod), null);

            if (!context.IsReplaying)
                _logger.LogInformation("[IncentivePaymentOrchestrator] Incentive Payment process started for collection period {collectionPeriod}", collectionPeriod);

            context.SetCustomStatus("SettingActivePeriodToInProgress");
            await context.CallActivityAsync(nameof(SetActivePeriodToInProgress), null);

            context.SetCustomStatus("GettingPayableLegalEntities");
            var payableLegalEntities = await context.CallActivityAsync<List<PayableLegalEntityDto>>(nameof(GetPayableLegalEntities), collectionPeriod);            

            context.SetCustomStatus("CalculatingPayments");
            var calculatePaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                var calculatePaymentTask = context.CallSubOrchestratorAsync(nameof(CalculatePaymentsForAccountLegalEntityOrchestrator), new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                calculatePaymentTasks.Add(calculatePaymentTask);
            }

            await Task.WhenAll(calculatePaymentTasks);

            context.SetCustomStatus("GettingUnsentClawbackLegalEntities");
            var clawbackLegalEntities = await context.CallActivityAsync<List<ClawbackLegalEntityDto>>(nameof(GetUnsentClawbacks), collectionPeriod);

            if (!context.IsReplaying)
            {
                context.SetCustomStatus("LoggingUnsentClawbacks");
                foreach (var legalEntity in clawbackLegalEntities)
                {
                    _logger.LogInformation($"Unsent clawback for AccountId : {legalEntity.AccountId}, AccountLegalEntityId : {legalEntity.AccountLegalEntityId}, Collection Year : {collectionPeriod.Year}, Period : {collectionPeriod.Period}");
                }
            }

            context.SetCustomStatus("WaitingForPaymentApproval");

            var paymentsApproved = await context.WaitForExternalEvent<bool>("PaymentsApproved");
            if (!paymentsApproved)
            {
                context.SetCustomStatus("PaymentsRejected");
                _logger.LogInformation("[IncentivePaymentOrchestrator] Calculated payments for collection period {collectionPeriod} have been rejected", collectionPeriod);
                return;
            }

            context.SetCustomStatus("SendingPayments");
            _logger.LogInformation("[IncentivePaymentOrchestrator] Calculated payments for collection period {collectionPeriod} have been approved", collectionPeriod);

            var sendPaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                var sendPaymentTask = context.CallActivityAsync(nameof(SendPaymentRequestsForAccountLegalEntity), new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                sendPaymentTasks.Add(sendPaymentTask);
            }
            await Task.WhenAll(sendPaymentTasks);
            context.SetCustomStatus("PaymentsSent");

            context.SetCustomStatus("SendingClawbacks");
            var sendClawbackTasks = new List<Task>();
            foreach (var legalEntity in clawbackLegalEntities)
            {
                var sendClawbackTask = context.CallActivityAsync(nameof(SendClawbacksForAccountLegalEntity), new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                sendClawbackTasks.Add(sendClawbackTask);
            }
            await Task.WhenAll(sendClawbackTasks);
            context.SetCustomStatus("ClawbacksSent");

            context.SetCustomStatus("CompletingPaymentProcessing");
            await context.CallActivityAsync(nameof(CompletePaymentProcess), new CompletePaymentProcessInput { CompletionDateTime = DateTime.UtcNow, CollectionPeriod = collectionPeriod });
            context.SetCustomStatus("PaymentProcessingCompleted");

            _logger.LogInformation("[IncentivePaymentOrchestrator] Incentive Payment process completed for collection period {collectionPeriod}", collectionPeriod);
        }
    }
}