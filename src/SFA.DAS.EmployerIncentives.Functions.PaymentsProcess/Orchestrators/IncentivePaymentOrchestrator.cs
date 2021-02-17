using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
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
            var collectionPeriod = context.GetInput<CollectionPeriod>();

            if (!context.IsReplaying)
                _logger.LogInformation("[IncentivePaymentOrchestrator] Incentive Payment process started for collection period {collectionPeriod}", collectionPeriod);

            context.SetCustomStatus("GettingPayableLegalEntities");
            var payableLegalEntities = await context.CallActivityAsync<List<PayableLegalEntityDto>>(nameof(GetPayableLegalEntities), collectionPeriod);
            _logger.LogInformation("[IncentivePaymentOrchestrator] Number of payable legal entities found: {PayableLegalEntities}", payableLegalEntities.Count);

            context.SetCustomStatus("CalculatingPayments");
            var calculatePaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                var calculatePaymentTask = context.CallSubOrchestratorAsync(nameof(CalculatePaymentsForAccountLegalEntityOrchestrator), new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                calculatePaymentTasks.Add(calculatePaymentTask);
            }

            await Task.WhenAll(calculatePaymentTasks);

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

            _logger.LogInformation("[IncentivePaymentOrchestrator] Incentive Payment process completed for collection period {collectionPeriod}", collectionPeriod);
        }
    }
}