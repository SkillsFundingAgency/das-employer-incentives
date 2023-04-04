using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingOrchestratorTimer
    {
        [FunctionName("LearnerMatchingOrchestrator_Timer")]
        public async Task Run([TimerTrigger("%LearnerMatchingTriggerTime%", RunOnStartup = false)] TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation("Auto Triggering LearnerMatchingOrchestrator");

            string instanceId = await starter.StartNewAsync(nameof(LearnerMatchingOrchestrator));

            log.LogInformation($"Auto Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");
        }
    }
}
