using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SetSuccessfulLearnerMatch
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SetSuccessfulLearnerMatch(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(SetSuccessfulLearnerMatch))]
        public async Task Set([ActivityTrigger] SetSuccessfulLearnerMatchInput input)
        {
            await _commandDispatcher.Send(new SetSuccessfulLearnerMatchCommand(input.ApprenticeshipIncentiveId, input.Uln, input.Succeeded));
        }
    }
}
