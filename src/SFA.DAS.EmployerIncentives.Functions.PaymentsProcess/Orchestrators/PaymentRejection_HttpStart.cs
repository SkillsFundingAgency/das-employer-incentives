using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class PaymentRejectionHttpStart
    {
        [FunctionName("PaymentRejection_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/IncentivePaymentOrchestrator/{instanceId}/rejectPayments")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string instanceId,
            ILogger log)
        {
            log.LogInformation($"Rejected payments for orchestrator instance {instanceId}");

            await starter.RaiseEventAsync(instanceId, "PaymentsApproved", false);

            log.LogInformation($"Rejected payments for orchestrator instance {instanceId}");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
