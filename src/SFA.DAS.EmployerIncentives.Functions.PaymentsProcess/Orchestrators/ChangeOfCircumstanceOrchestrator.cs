﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        [FunctionName(nameof(ChangeOfCircumstanceOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var learnerChangeOfCircumstanceInput = context.GetInput<LearnerChangeOfCircumstanceInput>();

            if (!context.IsReplaying)
                _logger.LogDebug("Learner Change of Circumstances process started for apprenticeship Incentive {apprenticeshipIncentiveId}, ULN: {uln}", learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln);

            var hasPossibleChangeOfCircs = await context.CallActivityAsync<bool>(nameof(ApprenticeshipIncentiveHasPossibleChangeOrCircs), learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId);

            if(hasPossibleChangeOfCircs)
            {
                await context.CallActivityAsync(nameof(LearnerChangeOfCircumstanceActivity), learnerChangeOfCircumstanceInput);
                await context.CallActivityWithRetryAsync(nameof(LearnerMatchAndUpdate), new RetryOptions(TimeSpan.FromSeconds(1), 3), new LearnerMatchInput { ApprenticeshipIncentiveId = learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId });
            }

            _logger.LogDebug("Learner Change of Circumstances process completed for apprenticeship Incentive {apprenticeshipIncentiveId}, ULN: {uln}", learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln);
        }
    }
}
