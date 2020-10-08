using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class IncentivePaymentOrchestrator
    {
        [FunctionName("IncentivePaymentOrchestrator")]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var collectionPeriod = context.GetInput<CollectionPeriod>();

            log.LogInformation($"Incentive Payment process started for collection period {collectionPeriod}", new { collectionPeriod });

            var payableLegalEntities = await context.CallActivityAsync<List<long>>("GetPayableLegalEntities", collectionPeriod);

            foreach (var legalEntity in payableLegalEntities)
            {
                log.LogInformation($"Request made to process payments for account legal entity {legalEntity}", new { collectionPeriod, legalEntity });
            }

            log.LogInformation($"Incentive Payment process completed for collection period {collectionPeriod}", new { collectionPeriod });
        }
    }
}