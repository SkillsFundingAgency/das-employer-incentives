using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public static class MonthEndOrchestrator_HttpStart
    {
        [FunctionName(nameof(MonthEndOrchestrator_HttpStart))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/MonthEndOrchestrator/{collectionPeriodYear}/{collectionPeriodMonth}")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            short collectionPeriodYear,
            byte collectionPeriodMonth,
            ILogger log)
        {
            const string subOrchestrator = nameof(MonthEndOrchestrator);

            var collectionPeriod = new CollectionPeriod { Year = collectionPeriodYear, Month = collectionPeriodMonth };

            var instanceId = await starter.StartNewAsync(subOrchestrator, null, collectionPeriod);

            log.LogInformation($"Started {subOrchestrator} instance with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
