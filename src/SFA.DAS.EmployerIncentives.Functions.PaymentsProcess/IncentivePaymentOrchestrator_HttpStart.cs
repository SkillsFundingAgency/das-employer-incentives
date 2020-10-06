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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/IncentivePaymentOrchestrator/{collectionPeriod}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string collectionPeriod,
            ILogger log)
        {
            log.LogInformation($"Triggering IncentivePaymentOrchestrator for collection period {collectionPeriod}");

            string instanceId = await starter.StartNewAsync("IncentivePaymentOrchestrator", null, collectionPeriod);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
