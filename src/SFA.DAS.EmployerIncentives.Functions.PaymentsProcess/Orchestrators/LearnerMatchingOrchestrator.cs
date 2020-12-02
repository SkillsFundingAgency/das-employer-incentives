using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

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
            if(!context.IsReplaying)
                _logger.LogInformation("LearnerMatchOrchestrator Started");

            var apprenticeshipIncentives = await context.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>(nameof(GetAllApprenticeshipIncentives), null);

            var matchingTasks = new List<Task>();
            foreach (var apprenticeshipIncentive in apprenticeshipIncentives)
            {
                var task = context
                    .CallActivityAsync(nameof(LearnerMatchAndUpdate),  new LearnerMatchInput {ApprenticeshipIncentiveId = apprenticeshipIncentive.Id})
                    .ContinueWith(x => context.CallSubOrchestratorAsync(nameof(ChangeOfCircumstanceOrchestrator), new LearnerChangeOfCircumstanceInput(apprenticeshipIncentive.Id, apprenticeshipIncentive.ULN)));
                matchingTasks.Add(task);
            }

            await Task.WhenAll(matchingTasks);

            matchingTasks = new List<Task>();

            var activePeriod = await context.CallActivityAsync<CollectionPeriodDto>(nameof(GetActiveCollectionPeriod), null);
            foreach (var apprenticeshipIncentive in apprenticeshipIncentives)
            {
                var task = context.CallActivityAsync(nameof(CalculateDaysInLearning), new CalculateDaysInLearningInput { ApprenticeshipIncentiveId = apprenticeshipIncentive.Id, ActivePeriod = activePeriod });
                matchingTasks.Add(task);
            }

            await Task.WhenAll(matchingTasks);

            _logger.LogInformation("Learner matching process completed");
        }
    }
}