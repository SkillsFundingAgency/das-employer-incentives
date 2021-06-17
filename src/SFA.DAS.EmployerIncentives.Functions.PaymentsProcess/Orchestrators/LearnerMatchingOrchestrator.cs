using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingOrchestrator
    {
        private readonly ILogger<LearnerMatchingOrchestrator> _logger;
        public LearnerMatchingOrchestrator(ILogger<LearnerMatchingOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(LearnerMatchingOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if(!context.IsReplaying) _logger.LogInformation("[LearnerMatchingOrchestrator] Learner matching process started");

            var apprenticeshipIncentives = await context.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>(nameof(GetAllApprenticeshipIncentives), null);
           
            _logger.LogInformation("[LearnerMatchingOrchestrator] {count} apprenticeship incentives found", apprenticeshipIncentives.Count);

            foreach (var incentive in apprenticeshipIncentives)
            {
                await context.CallSubOrchestratorAsync(nameof(LearnerMatchingApprenticeshipOrchestrator), incentive);
            }

            if(!context.IsReplaying) _logger.LogInformation("[LearnerMatchingOrchestrator] Learner matching process completed");
        }
    }
}