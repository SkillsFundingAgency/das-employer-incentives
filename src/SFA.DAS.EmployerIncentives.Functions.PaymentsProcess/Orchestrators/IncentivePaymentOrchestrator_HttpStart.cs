using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Threading.Tasks;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class IncentivePaymentOrchestratorHttpStart
    {
        private readonly ILogger _logger;

        public IncentivePaymentOrchestratorHttpStart(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IncentivePaymentOrchestratorHttpStart>();
        }

        [Function("IncentivePaymentOrchestrator_HttpStart")]
        public async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/IncentivePaymentOrchestrator")] HttpRequestData req,
            [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation($"Triggering IncentivePaymentOrchestrator for current active collection period");

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(IncentivePaymentOrchestrator), null);

            _logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
