using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleNotifyNewAgreementRequiredCommand
    {
        private readonly ICommandService _commandService;

        public HandleNotifyNewAgreementRequiredCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleNotifyNewAgreementRequiredCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.NotifyNewAgreementRequired)] NotifyNewAgreementRequiredCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
