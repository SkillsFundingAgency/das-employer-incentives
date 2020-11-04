using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public static class LearnerMatchingOrchestratorStart
    {
        [FunctionName("LearnerMatchingOrchestrator_Start")]
        public static async Task TimerStart(
#if DEBUG
            [TimerTrigger("0 */05 * * * *")] TimerInfo timerInfo,
#else
            [TimerTrigger("%LearnerMatchingOrchestratorTriggerTime%")] TimerInfo timerInfo,
#endif
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            log.LogInformation("Triggering LearnerMatchingOrchestrator");

            string instanceId = await starter.StartNewAsync("LearnerMatchingOrchestrator");

            log.LogInformation($"Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");
        }
    }
}
