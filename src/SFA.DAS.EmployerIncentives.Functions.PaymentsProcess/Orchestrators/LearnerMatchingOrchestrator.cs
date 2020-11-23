using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingOrchestrator
    {
        private ILogger<LearnerMatchingOrchestrator> _logger;

        public LearnerMatchingOrchestrator(ILogger<LearnerMatchingOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName("LearnerMatchingOrchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if(!context.IsReplaying)
                _logger.LogInformation("LearnerMatchOrchestrator Started");

            var apprenticeshipIncentives = await context.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>("GetAllApprenticeshipIncentives", null);

            var matchingTasks = new List<Task>();
            foreach (var apprenticeshipIncentive in apprenticeshipIncentives)
            {
                var task = context
                    .CallActivityAsync("LearnerMatchAndUpdate",  new LearnerMatchInput {ApprenticeshipIncentiveId = apprenticeshipIncentive.Id})
                    .ContinueWith(x => context.CallSubOrchestratorAsync("ChangeOfCircumstanceOrchestrator", new LearnerChangeOfCircumstanceInput(apprenticeshipIncentive.Id, apprenticeshipIncentive.ULN)));
                matchingTasks.Add(task);
            }

            await Task.WhenAll(matchingTasks);

            _logger.LogInformation("Learner matching process completed");
        }
    }
}