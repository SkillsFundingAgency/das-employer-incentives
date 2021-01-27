using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleUpdateVendorRegistrationCaseStatusForAccountCommand
    {
        private readonly ICommandService _commandService;

        public HandleUpdateVendorRegistrationCaseStatusForAccountCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleUpdateVendorRegistrationCaseStatusForAccountCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.UpdateVendorRegistrationCaseStatus)] UpdateVendorRegistrationCaseStatusForAccountCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
