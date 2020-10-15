using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

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

            await _logger.LogInformationAsync($"Incentive Payment process started for collection period {collectionPeriod}", new { collectionPeriod });

            var payableLegalEntities = await context.CallActivityAsync<List<long>>("GetPayableLegalEntities", collectionPeriod);

            var calculatePaymentTasks = new List<Task>();
            foreach (var legalEntity in payableLegalEntities)
            {
                await _logger.LogInformationAsync($"Request made to process payments for account legal entity {legalEntity}", new { collectionPeriod, legalEntity });
                var calculatePaymentTask = context.CallSubOrchestratorAsync("CalculatePaymentsForAccountLegalEntityOrchestrator", new AccountLegalEntityCollectionPeriod { AccountLegalEntityId = legalEntity, CollectionPeriod = collectionPeriod });
                calculatePaymentTasks.Add(calculatePaymentTask);
            }

            await Task.WhenAll(calculatePaymentTasks);

            await _logger.LogInformationAsync($"Incentive Payment process completed for collection period {collectionPeriod}", new { collectionPeriod });
        }
    }
}