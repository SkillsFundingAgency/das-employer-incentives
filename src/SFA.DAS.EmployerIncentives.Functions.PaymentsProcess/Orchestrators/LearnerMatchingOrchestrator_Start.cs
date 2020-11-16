using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public static class LearnerMatchingOrchestratorStart
    {
        private const string FarFuture = "0 0 0 29 2 1"; // Next Monday Feb 29 (29/02/2044)
        [FunctionName("LearnerMatchingOrchestrator_Start")]
        public static async Task TimerStart(
#if DEBUG
            [TimerTrigger(FarFuture, RunOnStartup = false)] TimerInfo timerInfo,
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
