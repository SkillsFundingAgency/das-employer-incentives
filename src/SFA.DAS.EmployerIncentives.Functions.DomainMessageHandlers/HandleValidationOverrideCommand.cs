using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleValidationOverrideCommand
    {
        private readonly ICommandService _commandService;

        public HandleValidationOverrideCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleValidationOverrideCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ValidationOverride)] ValidationOverrideCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
