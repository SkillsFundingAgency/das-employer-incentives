using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateDaysInLearning;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CalculateDaysInLearning
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public CalculateDaysInLearning(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(CalculateDaysInLearning))]
        public async Task Create([ActivityTrigger] CalculateDaysInLearningInput input)
        {
            await _commandDispatcher.Send(new CalculateDaysInLearningCommand(input.ApprenticeshipIncentiveId, input.ActivePeriod.Period, input.ActivePeriod.Year ));
        }
    }
}
