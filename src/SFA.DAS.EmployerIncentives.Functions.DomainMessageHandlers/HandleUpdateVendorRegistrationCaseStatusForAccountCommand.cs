using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleUpdateVendorRegistrationCaseStatusForAccountCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleUpdateVendorRegistrationCaseStatusForAccountCommand(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(HandleUpdateVendorRegistrationCaseStatusForAccountCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.UpdateVendorRegistrationCaseStatus)] UpdateVendorRegistrationCaseStatusForAccountCommand command)
        {
            await _commandDispatcher.Send(command);
        }
    }
}
