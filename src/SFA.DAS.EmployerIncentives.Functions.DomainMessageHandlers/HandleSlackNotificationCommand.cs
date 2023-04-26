using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleSlackNotificationCommand
    {
        private readonly ICommandService _commandService;

        public HandleSlackNotificationCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandleSlackNotificationCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.SlackNotificationCommand)] SlackNotificationCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
