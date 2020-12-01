using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class IncentivePaymentOrchestratorHttpStart
    {
        [FunctionName("IncentivePaymentOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriodNumber}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            short collectionPeriodYear,
            byte collectionPeriodNumber,
            ILogger log)
        {
            var collectionPeriod = new CollectionPeriod { Year = collectionPeriodYear, Period = collectionPeriodNumber };

            log.LogInformation($"Triggering IncentivePaymentOrchestrator for collection period {collectionPeriod}", new { collectionPeriod });

            string instanceId = await starter.StartNewAsync("IncentivePaymentOrchestrator", null, collectionPeriod);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
