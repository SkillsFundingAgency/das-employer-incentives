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
            var collectionPeriod = context.GetInput<string>();

            log.LogInformation($"Incentive Payment process started for collection period {collectionPeriod}", new { collectionPeriod });

            log.LogInformation($"Incentive Payment process completed for collection period {collectionPeriod}", new { collectionPeriod });
        }
    }
}