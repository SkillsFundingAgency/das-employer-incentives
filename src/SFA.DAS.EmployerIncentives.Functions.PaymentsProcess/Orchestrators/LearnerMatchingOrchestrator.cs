using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingOrchestrator
    {
        private ILogger<LearnerMatchingOrchestrator> _logger;

        public LearnerMatchingOrchestrator(ILogger<LearnerMatchingOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(LearnerMatchingOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (!context.IsReplaying)
                _logger.LogInformation("LearnerMatchOrchestrator Started");

            var apprenticeshipIncentives = await context.CallActivityAsync<List<Guid>>("GetAllApprenticeshipIncentives", null);

            var matchingTasks = new List<Task>();
            foreach (var apprenticeshipIncentiveId in apprenticeshipIncentives)
            {
                var task = context.CallActivityAsync("LearnerMatchAndUpdate", new LearnerMatchInput { ApprenticeshipIncentiveId = apprenticeshipIncentiveId });
                matchingTasks.Add(task);
            }

            await Task.WhenAll(matchingTasks);

            _logger.LogInformation("Learner matching process completed");
        }
    }
}