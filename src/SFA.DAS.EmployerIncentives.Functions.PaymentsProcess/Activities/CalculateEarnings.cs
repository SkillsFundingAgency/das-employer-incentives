using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CalculateEarningsActivity
    {
        private readonly ICommandDispatcher _commandDispatcher;
        
        public CalculateEarningsActivity(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(CalculateEarningsActivity))]
        public async Task Update([ActivityTrigger] CalculateEarningsInput input)
        {
            await _commandDispatcher.Send(new CalculateEarningsCommand(input.ApprenticeshipIncentiveId));
        }
    }
}
