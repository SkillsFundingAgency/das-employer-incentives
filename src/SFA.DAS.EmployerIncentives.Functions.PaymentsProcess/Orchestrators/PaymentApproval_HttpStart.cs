using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class PaymentApprovalHttpStart
    {
        [Function("PaymentApproval_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/approvePayments/{instanceId}")] HttpRequestData req,
            [DurableClient] DurableTaskClient starter,
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
