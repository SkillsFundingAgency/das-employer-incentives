using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var payableLegalEntities = await context.CallActivityAsync<List<PayableLegalEntity>>(nameof(GetPayableLegalEntities), collectionPeriod);
            context.SetCustomStatus("GettingUnsentClawbackLegalEntities");
            var clawbackLegalEntities = await context.CallActivityAsync<List<ClawbackLegalEntity>>(nameof(GetUnsentClawbacks), collectionPeriod);
            
            var accountLegalEntitiesToProcess = payableLegalEntities.Select(payableLegalEntity => new AccountLegalEntityCollectionPeriod {AccountId = payableLegalEntity.AccountId, AccountLegalEntityId = payableLegalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod}).ToList();
            foreach (var clawbackLegalEntity in clawbackLegalEntities.Where(clawbackLegalEntity => accountLegalEntitiesToProcess.FirstOrDefault(x => x.AccountId == clawbackLegalEntity.AccountId && x.AccountLegalEntityId == clawbackLegalEntity.AccountLegalEntityId) == null))
            {
                accountLegalEntitiesToProcess.Add(new AccountLegalEntityCollectionPeriod { AccountId = clawbackLegalEntity.AccountId, AccountLegalEntityId = clawbackLegalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
            }

            context.SetCustomStatus("CalculatingPayments");
            var calculatePaymentTasks = new List<Task>();
            foreach (var accountLegalEntityPaymentPeriod in accountLegalEntitiesToProcess)
            {
                var calculatePaymentTask = context.CallSubOrchestratorAsync(nameof(CalculatePaymentsForAccountLegalEntityOrchestrator), accountLegalEntityPaymentPeriod);
                calculatePaymentTasks.Add(calculatePaymentTask);
            }

            await Task.WhenAll(calculatePaymentTasks);

            if (!context.IsReplaying)
            {
                context.SetCustomStatus("LoggingUnsentClawbacks");
                foreach (var legalEntity in clawbackLegalEntities)
                {
                    _logger.LogDebug($"Unsent clawback for AccountId : {legalEntity.AccountId}, AccountLegalEntityId : {legalEntity.AccountLegalEntityId}, Collection Year : {collectionPeriod.Year}, Period : {collectionPeriod.Period}");
                }
            }

            if(!context.IsReplaying)
                _logger.LogInformation("[IncentivePaymentOrchestrator] Setting status to WaitingForPaymentApproval.");

            context.SetCustomStatus("WaitingForPaymentApproval");

            var paymentsApproved = await context.WaitForExternalEvent<bool>("PaymentsApproved");
            if (!paymentsApproved)
            {
                context.SetCustomStatus("PaymentsRejected");
                _logger.LogInformation("[IncentivePaymentOrchestrator] Calculated payments for collection period {collectionPeriod} have been rejected", collectionPeriod);
                return;
            }
            
            context.SetCustomStatus("SendingClawbacksAndPayments");
            _logger.LogInformation("[IncentivePaymentOrchestrator] Calculated payments for collection period {collectionPeriod} have been approved", collectionPeriod);

            var sendPaymentTasks = new List<Task>();
            foreach (var legalEntity in accountLegalEntitiesToProcess)
            {
                var sendPaymentTask = context.CallSubOrchestratorAsync(nameof(SendPaymentsForAccountLegalEntityOrchestrator), new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                sendPaymentTasks.Add(sendPaymentTask);
            }
            await Task.WhenAll(sendPaymentTasks);
            context.SetCustomStatus("ClawbacksAndPaymentsSent");

            context.SetCustomStatus("CompletingPaymentProcessing");
            await context.CallActivityAsync(nameof(CompletePaymentProcess), new CompletePaymentProcessInput { CompletionDateTime = DateTime.UtcNow, CollectionPeriod = collectionPeriod });
            context.SetCustomStatus("PaymentProcessingCompleted");

            _logger.LogInformation("[IncentivePaymentOrchestrator] Incentive Payment process completed for collection period {collectionPeriod}", collectionPeriod);
        }
    }
}