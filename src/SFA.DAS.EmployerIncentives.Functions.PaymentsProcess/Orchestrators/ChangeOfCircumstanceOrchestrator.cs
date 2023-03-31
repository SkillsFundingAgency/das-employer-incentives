using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class ChangeOfCircumstanceOrchestrator
    {
        private ILogger<ChangeOfCircumstanceOrchestrator> _logger;

        public ChangeOfCircumstanceOrchestrator(ILogger<ChangeOfCircumstanceOrchestrator> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ChangeOfCircumstanceOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var learnerChangeOfCircumstanceInput = context.GetInput<LearnerChangeOfCircumstanceInput>();

            if (!context.IsReplaying)
                _logger.LogDebug("Learner Change of Circumstances process started for apprenticeship Incentive {apprenticeshipIncentiveId}, ULN: {uln}", learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln);

            var hasPossibleChangeOfCircs = await context.CallActivityAsync<bool>(nameof(ApprenticeshipIncentiveHasPossibleChangeOrCircs), learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId);

            if(hasPossibleChangeOfCircs)
            {
                await context.CallActivityAsync(nameof(LearnerChangeOfCircumstanceActivity), learnerChangeOfCircumstanceInput);                
                await context.CallActivityAsync(nameof(LearnerMatchAndUpdate), new LearnerMatchInput { ApprenticeshipIncentiveId = learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId }, new TaskOptions(new TaskRetryOptions( new RetryPolicy(3, TimeSpan.FromSeconds(1)))));
            }

            _logger.LogDebug("Learner Change of Circumstances process completed for apprenticeship Incentive {apprenticeshipIncentiveId}, ULN: {uln}", learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln);
        }
    }
}
