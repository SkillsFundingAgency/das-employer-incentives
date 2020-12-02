using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
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

            if(!context.IsReplaying)
                _logger.LogInformation("Incentive Payment process started for collection period {collectionPeriod}", collectionPeriod);

            var payableLegalEntities = await context.CallActivityAsync<List<PayableLegalEntityDto>>(nameof(GetPayableLegalEntities), collectionPeriod);

            var calculatePaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                var calculatePaymentTask = context.CallSubOrchestratorAsync(nameof(CalculatePaymentsForAccountLegalEntityOrchestrator), new AccountLegalEntityCollectionPeriod { AccountId = legalEntity.AccountId, AccountLegalEntityId = legalEntity.AccountLegalEntityId, CollectionPeriod = collectionPeriod });
                calculatePaymentTasks.Add(calculatePaymentTask);
            }

            await Task.WhenAll(calculatePaymentTasks);

            _logger.LogInformation("Incentive Payment process completed for collection period {collectionPeriod}", collectionPeriod);
        }
    }
}