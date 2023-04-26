using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SendSlackNotification
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SendSlackNotification(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(SendSlackNotification))]
        public async Task Complete([ActivityTrigger] SendSlackNotificationInput input)
        {
            try
            {
                var command = new SlackNotificationCommand(input.Message);
                await _commandDispatcher.Send(command);
            }
            catch { }
        }
    }
}
