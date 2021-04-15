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

            var collectionPeriod = await context.CallActivityAsync<CollectionPeriodDto>(nameof(GetActiveCollectionPeriod), null);
            if (collectionPeriod.IsInProgress)
            {
                _logger.LogInformation("Learner match not performed as payment run is in process.");
                return;
            }

            var apprenticeshipIncentives = await context.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>(nameof(GetAllApprenticeshipIncentives), null);

            await PerformLearnerMatch(context, apprenticeshipIncentives);

            await PerformChangeOfCircumstances(context, apprenticeshipIncentives);

            await CalculateDaysInLearning(context, apprenticeshipIncentives);

            _logger.LogInformation("Learner matching process completed");
        }

        private static async Task CalculateDaysInLearning(IDurableOrchestrationContext context, List<ApprenticeshipIncentiveOutput> apprenticeshipIncentives)
        {
            var matchingTasks = new List<Task>();

            var activePeriod = await context.CallActivityAsync<CollectionPeriodDto>(nameof(GetActiveCollectionPeriod), null);
            foreach (var apprenticeshipIncentive in apprenticeshipIncentives)
            {
                var task = context.CallActivityAsync(nameof(Activities.CalculateDaysInLearning),
                    new CalculateDaysInLearningInput
                        {ApprenticeshipIncentiveId = apprenticeshipIncentive.Id, ActivePeriod = activePeriod});
                matchingTasks.Add(task);
            }

            await Task.WhenAll(matchingTasks);
        }

        private static async Task PerformChangeOfCircumstances(IDurableOrchestrationContext context, List<ApprenticeshipIncentiveOutput> apprenticeshipIncentives)
        {
            var matchingTasks = new List<Task>();
            foreach (var apprenticeshipIncentive in apprenticeshipIncentives)
            {
                var task = context.CallSubOrchestratorAsync(nameof(ChangeOfCircumstanceOrchestrator),
                    new LearnerChangeOfCircumstanceInput(apprenticeshipIncentive.Id, apprenticeshipIncentive.ULN));
                matchingTasks.Add(task);
            }
            await Task.WhenAll(matchingTasks);
        }

        private static async Task PerformLearnerMatch(IDurableOrchestrationContext context, List<ApprenticeshipIncentiveOutput> apprenticeshipIncentives)
        {
            var matchingTasks = new List<Task>();
            foreach (var apprenticeshipIncentive in apprenticeshipIncentives)
            {
                var task = context.CallActivityWithRetryAsync(nameof(LearnerMatchAndUpdate), new RetryOptions(TimeSpan.FromSeconds(1), 3), 
                    new LearnerMatchInput {ApprenticeshipIncentiveId = apprenticeshipIncentive.Id});
                matchingTasks.Add(task);
            }

            await Task.WhenAll(matchingTasks);
        }
    }
}