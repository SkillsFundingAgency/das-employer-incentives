using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CalculateEarningsActivity
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private ILogger<CalculateEarningsActivity> _logger;

        public CalculateEarningsActivity(ICommandDispatcher commandDispatcher, ILogger<CalculateEarningsActivity> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(CalculateEarningsActivity))]
        public async Task Update([ActivityTrigger] CalculateEarningsInput input)
        {
            await _commandDispatcher.Send(new CalculateEarningsCommand(input.ApprenticeshipIncentiveId));
        }
    }
}
