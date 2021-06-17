using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class LearnerMatchingApprenticeshipOrchestrator
    {
        private readonly ILogger<LearnerMatchingApprenticeshipOrchestrator> _logger;

        public LearnerMatchingApprenticeshipOrchestrator(ILogger<LearnerMatchingApprenticeshipOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(LearnerMatchingApprenticeshipOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var incentive = context.GetInput<ApprenticeshipIncentiveOutput>();

            if (!context.IsReplaying)
                _logger.LogInformation("LearnerMatchingApprenticeshipOrchestrator Started for Apprenticeship: {incentive}", incentive);

            await PerformLearnerMatch(context, incentive);

            await PerformChangeOfCircumstances(context, incentive);

            await CalculateDaysInLearning(context, incentive);

            if (!context.IsReplaying)
                _logger.LogInformation("LearnerMatchingApprenticeshipOrchestrator Completed for Apprenticeship: {incentive}", incentive);
        }

        private static async Task CalculateDaysInLearning(IDurableOrchestrationContext context, ApprenticeshipIncentiveOutput incentive)
        {
            var activePeriod = await context.CallActivityAsync<CollectionPeriod>(nameof(GetActiveCollectionPeriod), null);
            
            await context.CallActivityAsync(nameof(Activities.CalculateDaysInLearning),
                new CalculateDaysInLearningInput {ApprenticeshipIncentiveId = incentive.Id, ActivePeriod = activePeriod});
        }

        private static async Task PerformChangeOfCircumstances(IDurableOrchestrationContext context, ApprenticeshipIncentiveOutput incentive)
        {
            await context.CallSubOrchestratorAsync(nameof(ChangeOfCircumstanceOrchestrator),
                new LearnerChangeOfCircumstanceInput(incentive.Id, incentive.ULN));
        }

        private static async Task PerformLearnerMatch(IDurableOrchestrationContext context, ApprenticeshipIncentiveOutput incentive)
        {
            await context.CallActivityWithRetryAsync(nameof(LearnerMatchAndUpdate),
                new RetryOptions(TimeSpan.FromSeconds(1), 3),
                new LearnerMatchInput {ApprenticeshipIncentiveId = incentive.Id});
        }
    }
}
