using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class LearnerMatchAndUpdate
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<LearnerMatchAndUpdate> _logger;

        public LearnerMatchAndUpdate(ICommandDispatcher commandDispatcher, ILogger<LearnerMatchAndUpdate> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName("LearnerMatchAndUpdate")]
        public async Task Create([ActivityTrigger] LearnerMatchInput input)
        {
            _logger.LogInformation("Creating Learner Match record for apprenticeship incentive id {apprenticeshipIncentiveId}", input.ApprenticeshipIncentiveId);
            await _commandDispatcher.Send(new RefreshLearnerCommand(input.ApprenticeshipIncentiveId));
            _logger.LogInformation("Created Learner Match record for apprenticeship incentive id {apprenticeshipIncentiveId}", input.ApprenticeshipIncentiveId);
        }
    }
}
