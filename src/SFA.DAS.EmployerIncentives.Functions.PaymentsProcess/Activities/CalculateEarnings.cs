using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CalculateEarnings
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private ILogger<CalculateEarnings> _logger;

        public CalculateEarnings(ICommandDispatcher commandDispatcher, ILogger<CalculateEarnings> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName("CalculateEarningsActivity")]
        public async Task Update([ActivityTrigger] CalculateEarningsInput input)
        {
            _logger.LogInformation("Calculating Earnings for apprenticeship incentive id {apprenticeshipIncentiveId}, uln {uln}", input.ApprenticeshipIncentiveId, input.Uln);
            await _commandDispatcher.Send(new CalculateEarningsCommand(input.ApprenticeshipIncentiveId));
            _logger.LogInformation("Calculated Earnings for apprenticeship incentive id {apprenticeshipIncentiveId}, uln {uln}", input.ApprenticeshipIncentiveId, input.Uln);
        }
    }
}
