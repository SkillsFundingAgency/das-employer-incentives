using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingOrchestratorStart
    {
        private readonly ILogger _logger;

        public LearnerMatchingOrchestratorStart(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LearnerMatchingOrchestratorStart>();
        }

        [Function("LearnerMatchingOrchestrator_Start")]
        public async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/LearnerMatchingOrchestrator")] HttpRequestData req,
            [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation("Manually Triggering LearnerMatchingOrchestrator");

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(LearnerMatchingOrchestrator));

            _logger.LogInformation($"Manually Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
