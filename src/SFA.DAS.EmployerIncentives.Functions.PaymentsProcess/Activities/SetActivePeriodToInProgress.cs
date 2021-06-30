using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar.SetActivePeriodToInProgress;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SetActivePeriodToInProgress
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private ILogger<SetActivePeriodToInProgress> _logger;

        public SetActivePeriodToInProgress(ICommandDispatcher commandDispatcher, ILogger<SetActivePeriodToInProgress> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(SetActivePeriodToInProgress))]
        public async Task Update([ActivityTrigger] object input)
        {
            _logger.LogInformation("Setting active collection period to in progress");
            await _commandDispatcher.Send(new SetActivePeriodToInProgressCommand());
            _logger.LogInformation("Active collection period set to in progress");
        }
    }
}
