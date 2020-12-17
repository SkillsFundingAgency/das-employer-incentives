using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public static class LearnerMatchingOrchestratorStart
    {
        [FunctionName("LearnerMatchingOrchestrator_Start")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/LearnerMatchingOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            log.LogInformation("Triggering LearnerMatchingOrchestrator");

            string instanceId = await starter.StartNewAsync(nameof(LearnerMatchingOrchestrator));

            log.LogInformation($"Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
