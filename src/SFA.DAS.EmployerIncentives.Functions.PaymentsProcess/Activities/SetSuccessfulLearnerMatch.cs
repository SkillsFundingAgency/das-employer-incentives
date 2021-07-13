﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SetSuccessfulLearnerMatch
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<SetSuccessfulLearnerMatch> _logger;

        public SetSuccessfulLearnerMatch(ICommandDispatcher commandDispatcher, ILogger<SetSuccessfulLearnerMatch> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(SetSuccessfulLearnerMatch))]
        public async Task Set([ActivityTrigger] SetSuccessfulLearnerMatchInput input)
        {
            _logger.LogInformation("Setting SuccessfulLearnerMatch for apprenticeship incentive id {apprenticeshipIncentiveId}, ULN {uln}, Succeeded: {succeeded}", input.ApprenticeshipIncentiveId, input.Uln, input.Succeeded);
            await _commandDispatcher.Send(new SetSuccessfulLearnerMatchCommand(input.ApprenticeshipIncentiveId, input.Uln, input.Succeeded));
            _logger.LogInformation("Set SuccessfulLearnerMatch for apprenticeship incentive id {apprenticeshipIncentiveId}, ULN {uln}, Succeeded: {succeeded}", input.ApprenticeshipIncentiveId, input.Uln, input.Succeeded);
        }
    }
}