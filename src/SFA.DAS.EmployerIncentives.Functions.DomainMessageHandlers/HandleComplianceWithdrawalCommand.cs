using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleComplianceWithdrawalCommand
    {
        private readonly ICommandService _commandService;

        public HandleComplianceWithdrawalCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleComplianceWithdrawalCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ComplianceWithdrawal)] ComplianceWithdrawalCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
