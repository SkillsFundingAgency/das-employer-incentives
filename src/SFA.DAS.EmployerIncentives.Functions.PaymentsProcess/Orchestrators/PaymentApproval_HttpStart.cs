using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class PaymentApprovalHttpStart
    {
        [FunctionName("PaymentApproval_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/IncentivePaymentOrchestrator/{instanceId}/approvePayments")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string instanceId,
            ILogger log)
        {
            log.LogInformation($"Approving payments for orchestrator instance {instanceId}");

            await starter.RaiseEventAsync(instanceId, "PaymentsApproved", true);

            log.LogInformation($"Approved payments for orchestrator instance {instanceId}");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
