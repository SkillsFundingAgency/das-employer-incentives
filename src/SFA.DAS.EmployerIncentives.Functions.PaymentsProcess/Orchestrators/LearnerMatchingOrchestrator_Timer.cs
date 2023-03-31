using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingOrchestratorTimer
    {
        private readonly ILogger _logger;

        public LearnerMatchingOrchestratorTimer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LearnerMatchingOrchestratorTimer>();
        }

        [Function("LearnerMatchingOrchestrator_Timer")]
        public async Task Run([TimerTrigger("%LearnerMatchingTriggerTime%", RunOnStartup = false)] TimerInfo myTimer, [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation("Auto Triggering LearnerMatchingOrchestrator");

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(LearnerMatchingOrchestrator));

            _logger.LogInformation($"Auto Started LearnerMatchingOrchestrator with ID = '{instanceId}'.");
        }
    }
}
