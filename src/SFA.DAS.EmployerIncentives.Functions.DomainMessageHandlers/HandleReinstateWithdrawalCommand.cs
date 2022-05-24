using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleReinstateWithdrawalCommand
    {
        private readonly ICommandService _commandService;

        public HandleReinstateWithdrawalCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleReinstateWithdrawalCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ReinstateWithdrawal)] ReinstateWithdrawalCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
