using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System.Collections.Generic;
using System.Linq;
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

            var collectionPeriod = await context.CallActivityAsync<CollectionPeriod>(nameof(GetActiveCollectionPeriod), null);
            if (collectionPeriod.IsInProgress)
            {
                _logger.LogInformation("Learner match not performed as payment run is in process.");
                return;
            }

            var apprenticeshipIncentives = await context.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>(nameof(GetAllApprenticeshipIncentives), null);
           
            _logger.LogDebug("[LearnerMatchingOrchestrator] {count} apprenticeship incentives found", apprenticeshipIncentives.Count);

            var tasks = apprenticeshipIncentives.Select(incentive => context.CallSubOrchestratorAsync(nameof(LearnerMatchingApprenticeshipOrchestrator), incentive)).ToList();
            await Task.WhenAll(tasks);

            if (!context.IsReplaying) _logger.LogInformation("[LearnerMatchingOrchestrator] Learner matching process completed");
        }
    }
}