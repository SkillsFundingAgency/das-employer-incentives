using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public static class LearnerMatchingOrchestrator_HttpStart
    {
        [FunctionName("LearnerMatchingOrchestrator_HttpStart")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/LearnerMatchingOrchestrator/")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {

            log.LogInformation($"Triggering LearnerMatchingOrchestrator at {DateTime.UtcNow.ToShortDateString()}");

            var instanceId = await client.StartNewAsync("LearnerMatchingOrchestrator");

            log.LogInformation($"Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");

            return new OkObjectResult(instanceId);
        }
    }
}
