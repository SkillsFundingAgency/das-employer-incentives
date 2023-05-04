using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class PaymentApprovalResendEmailHttpStart
    {
        [FunctionName("PaymentApprovalResendEmail_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/approvePayments/{instanceId}/resendemail/{emailAddress}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string instanceId,
            string emailAddress,
            ILogger log)
        {
            log.LogInformation($"Querying provided email for orchestrator instance {instanceId}");

            var instance = await starter.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                RuntimeStatus = new[] { OrchestrationRuntimeStatus.Pending, OrchestrationRuntimeStatus.Running, OrchestrationRuntimeStatus.ContinuedAsNew },
                TaskHubNames = new[] { starter.TaskHubName },
                InstanceIdPrefix = instanceId,
            }, CancellationToken.None);

            if(instance.DurableOrchestrationState.Count() != 1 )
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"IncentivePaymentOrchestrator not found for instanceId {instanceId}") };
            }

            var paymentOrchestratorStatus = await starter.GetStatusAsync(instanceId, true, false, false);
            if (paymentOrchestratorStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"IncentivePaymentOrchestrator not running for instanceId {instanceId}") };
            }

            var paymentOrchestratorEvents = JsonSerializer.Deserialize<List<HistoryEvent>>(paymentOrchestratorStatus.History.ToString());

            var approvalsOrchestrator = paymentOrchestratorEvents.SingleOrDefault(h => h.Name == "PaymentApprovalsOrchestrator" && h.EventType == "SubOrchestrationInstanceCreated");
            if (approvalsOrchestrator == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"PaymentApprovalsOrchestrator not found (or started yet) for IncentivePaymentOrchestrator with instanceId {instanceId}") };
            }

            var approvalsOrchestratorStatus = await starter.GetStatusAsync(approvalsOrchestrator.InstanceId, true, false, false);
            if (approvalsOrchestratorStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"PaymentApprovalsOrchestrator not running for IncentivePaymentOrchestrator with instanceId {instanceId}") };
            }

            var approvalsOrchestratorEvents = JsonSerializer.Deserialize<List<HistoryEvent>>(approvalsOrchestratorStatus.History.ToString());

            var approvalOrchestrators = approvalsOrchestratorEvents.Where(h => h.Name == "PaymentApprovalOrchestrator" && h.EventType == "SubOrchestrationInstanceCreated");
            foreach(var approvalOrchestrator in approvalOrchestrators)
            {
                var approvalOrchestratorStatus = await starter.GetStatusAsync(approvalOrchestrator.InstanceId, true, true, true);
                if(approvalOrchestratorStatus.RuntimeStatus !=  OrchestrationRuntimeStatus.Running)
                {
                    continue;
                }

                var settings = new JsonSerializerOptions();
                settings.Converters.Add(new PaymentApprovalInputConverter());
                settings.Converters.Add(new PaymentApprovalResultConverter());                

                var approvalOrchestratorInput = JsonSerializer.Deserialize<PaymentApprovalInput>(approvalOrchestratorStatus.Input.ToString(), settings);

                if(approvalOrchestratorInput.PaymentOrchestrationId == instanceId &&
                   approvalOrchestratorInput.EmailAddress.Equals(emailAddress, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (approvalOrchestratorStatus.Output.HasValues)
                    {
                        var approvalOrchestratorOutput = JsonSerializer.Deserialize<PaymentApprovalResult>(approvalOrchestratorStatus.Output.ToString(), settings);
                        if (approvalOrchestratorOutput?.PaymentApprovalStatus == PaymentApprovalStatus.Approved)
                        {
                            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"The payment process has already been approved for the provided email for IncentivePaymentOrchestrator with instanceId {instanceId} so an email can not be resent") };
                        }
                        if (approvalOrchestratorOutput?.PaymentApprovalStatus == PaymentApprovalStatus.NotApprovedInTime)
                        {
                            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"The payment process has not been approved in time for the provided email for IncentivePaymentOrchestrator with instanceId {instanceId}  so an email can not be resent") };
                        }
                    }

                    approvalOrchestratorInput.IsResend = true;
                    approvalOrchestratorInput.PaymentApprovalOrchestrationId = approvalOrchestrator.InstanceId;
                    await starter.StartNewAsync(nameof(PaymentApprovalSendEmailOrchestrator), null, approvalOrchestratorInput);
                    log.LogInformation($"Resend email requested  for orchestrator instance {instanceId} using provided email");
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted) { Content = new StringContent($"Resend email successfully requested for orchestrator instance {instanceId} using provided email") };
                } 
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"A running PaymentApprovalOrchestrator was not found for provided email for IncentivePaymentOrchestrator with instanceId {instanceId}") };
        }

        class HistoryEvent
        {
            public string EventType { get; set; }
            public string Name { get; set; }
            public string InstanceId { get; set; }
        }  
        
        public class PaymentApprovalInputConverter : JsonConverter<PaymentApprovalInput>
        {
            public override PaymentApprovalInput Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options);

                var paymentApprovalInput = new PaymentApprovalInput(
                    JsonSerializer.Deserialize<CollectionPeriod>(dictionary["CollectionPeriod"].GetRawText(), options),
                    JsonSerializer.Deserialize<string>(dictionary["EmailAddress"].GetRawText(), options),
                    JsonSerializer.Deserialize<string>(dictionary["PaymentOrchestrationId"].GetRawText(), options),
                    JsonSerializer.Deserialize<string>(dictionary["CorrelationId"].GetRawText(), options)
                    )
                {
                    IsResend = JsonSerializer.Deserialize<bool>(dictionary["IsResend"].GetRawText(), options)
                };

                return paymentApprovalInput;
            }

            public override void Write(Utf8JsonWriter writer, PaymentApprovalInput value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        public class PaymentApprovalResultConverter : JsonConverter<PaymentApprovalResult>
        {
            public override PaymentApprovalResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options);

                var paymentApprovalResult = new PaymentApprovalResult
                {
                    EmailAddress = JsonSerializer.Deserialize<string>(dictionary["EmailAddress"].GetRawText(), options),
                    PaymentApprovalStatus = JsonSerializer.Deserialize<PaymentApprovalStatus>(dictionary["PaymentApprovalStatus"].GetRawText(), options)
                };

                return paymentApprovalResult;
            }

            public override void Write(Utf8JsonWriter writer, PaymentApprovalResult value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}
