using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public static class LearnerMatchingOrchestratorStart
    {
        [FunctionName("LearnerMatchingOrchestrator_Start")]
        public static async Task HttpStart(
            [TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            if (timerInfo.IsPastDue)
            {
                log.LogInformation("Running late");
            }

            log.LogInformation($"Triggering LearnerMatchingOrchestrator at {DateTime.UtcNow.ToShortDateString()}");

            string instanceId = await starter.StartNewAsync("LearnerMatchingOrchestrator");

            log.LogInformation($"Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");
        }
    }
}
