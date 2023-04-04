using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        [FunctionName(nameof(CalculateEarningsActivity))]
        public async Task Update([ActivityTrigger] CalculateEarningsInput input)
        {
            await _commandDispatcher.Send(new CalculateEarningsCommand(input.ApprenticeshipIncentiveId));
        }
    }
}
