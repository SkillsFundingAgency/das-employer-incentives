using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateDaysInLearning;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CalculateDaysInLearning
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<CalculateDaysInLearning> _logger;

        public CalculateDaysInLearning(ICommandDispatcher commandDispatcher, ILogger<CalculateDaysInLearning> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(CalculateDaysInLearning))]
        public async Task Create([ActivityTrigger] CalculateDaysInLearningInput input)
        {
            _logger.LogInformation("Calculating DaysinLearning for apprenticeship incentive id {apprenticeshipIncentiveId}, active period {activePeriod}", input.ApprenticeshipIncentiveId, input.ActivePeriod);
            await _commandDispatcher.Send(new CalculateDaysInLearningCommand(input.ApprenticeshipIncentiveId, input.ActivePeriod.CollectionPeriodNumber, input.ActivePeriod.CollectionYear ));
            _logger.LogInformation("Calculated DaysinLearning for apprenticeship incentive id {apprenticeshipIncentiveId}, active period {activePeriod}", input.ApprenticeshipIncentiveId, input.ActivePeriod);
        }
    }
}
