using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class OrchestrationStatusCheck
    {
        [FunctionName("GetStatus")]
        public static async Task<HttpResponseMessage> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/GetStatus/{instanceId}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient client,
            string instanceId,
            ILogger logger)
        {
            var status = await client.GetStatusAsync(instanceId);
           
            logger.LogInformation(JsonConvert.SerializeObject(status, Formatting.Indented));
            
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(status, Formatting.Indented)) };
        }

        [FunctionName("GetStatusFunc")]
        public static async Task GetStatusFunc(
            [DurableClient] IDurableOrchestrationClient client,
            [ActivityTrigger] string instanceId,
            ILogger logger)
        {
            var status = await client.GetStatusAsync(instanceId);

            logger.LogInformation(JsonConvert.SerializeObject(status, Formatting.Indented));
        }

        [FunctionName("GetAllStatus")]
        public static async Task<HttpResponseMessage> GetAllStatus(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/GetAllStatus")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var noFilter = new OrchestrationStatusQueryCondition();
            var result = await starter.ListInstancesAsync(
                noFilter,
                CancellationToken.None);

            string res = "";

            foreach (var instance in result.DurableOrchestrationState)
            {
                res += JsonConvert.SerializeObject(instance, Formatting.Indented);
                res += Environment.NewLine;
                log.LogInformation(JsonConvert.SerializeObject(instance));
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(res) };


            // Note: ListInstancesAsync only returns the first page of results.
            // To request additional pages provide the result.ContinuationToken
            // to the OrchestrationStatusQueryCondition's ContinuationToken property.
        }
    }
}
