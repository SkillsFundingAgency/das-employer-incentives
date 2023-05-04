using DurableTask.Core.History;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class PaymentApprovalHttpStart
    {
        [FunctionName("PaymentApproval_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/approvePayments/{instanceId}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string instanceId,
            ILogger log)
        {
            
            log.LogInformation($"Approving payments for orchestrator instance {instanceId}");

            if (instanceId.Contains("_"))
            {
                var instanceParts = instanceId.Split("_");

                try
                {
                    await starter.RaiseEventAsync(instanceParts[0], $"PaymentsApproved_{instanceParts[0]}_{instanceParts[1]}", true);
                }
                catch(Exception ex)
                {
                    log.LogError(ex, "Payment approval request can not be processed");
                    return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent($"Payment approval request can not be processed : {ex.Message}.") };
                }

                return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted) { Content = new StringContent($"Payment approval request has been received and will be processed.") };
            }
            else
            {
                await CancelRunningApprovals(starter, instanceId, log);

                await starter.RaiseEventAsync(instanceId, "PaymentsApproved", true);

                log.LogInformation($"Approved payments for orchestrator instance {instanceId}");

                return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted) { Content = new StringContent($"Approve payments for orchestrator instance {instanceId} has been received and will be processed.") };
            }
        }

        private static async Task CancelRunningApprovals(
            IDurableOrchestrationClient client, 
            string instanceId,
            ILogger log)
        {
            try
            {
                var instance = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
                {
                    RuntimeStatus = new[] { OrchestrationRuntimeStatus.Pending, OrchestrationRuntimeStatus.Running, OrchestrationRuntimeStatus.ContinuedAsNew },
                    TaskHubNames = new[] { client.TaskHubName },
                    InstanceIdPrefix = instanceId,
                }, CancellationToken.None);

                if (instance.DurableOrchestrationState.Count() == 1)
                {

                    var paymentOrchestratorStatus = await client.GetStatusAsync(instanceId, true, false, false);

                    var paymentOrchestratorEvents = JsonSerializer.Deserialize<List<HistoryEvent>>(paymentOrchestratorStatus.History.ToString());

                    var approvalsOrchestrator = paymentOrchestratorEvents.SingleOrDefault(h => h.Name == "PaymentApprovalsOrchestrator" && h.EventType == "SubOrchestrationInstanceCreated");
                    if (approvalsOrchestrator != null)
                    {
                        var approvalsOrchestratorStatus = await client.GetStatusAsync(approvalsOrchestrator.InstanceId, true, false, false);

                        var approvalsOrchestratorEvents = JsonSerializer.Deserialize<List<HistoryEvent>>(approvalsOrchestratorStatus.History.ToString());

                        var approvalOrchestrators = approvalsOrchestratorEvents.Where(h => h.Name == "PaymentApprovalOrchestrator" && h.EventType == "SubOrchestrationInstanceCreated");

                        foreach (var approvalOrchestrator in approvalOrchestrators)
                        {
                            await client.TerminateAsync(approvalOrchestrator.InstanceId, "Payments have been approved by support calling the approve payments endpoint");
                        }

                        await client.TerminateAsync(approvalsOrchestrator.InstanceId, "Payments have been approved by support calling the approve payments endpoint");
                    }
                }
            }
            catch(Exception ex)
            {
                log.LogError($"Unable to stop approval email process.  Please cancel tasks manually : {ex.Message} ");
            }
        }

        class HistoryEvent
        {
            public string EventType { get; set; }
            public string Name { get; set; }
            public string InstanceId { get; set; }
        }
    }
}
