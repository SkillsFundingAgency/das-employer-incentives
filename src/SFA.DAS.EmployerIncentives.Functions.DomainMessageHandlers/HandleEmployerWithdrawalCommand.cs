using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleEmployerWithdrawalCommand
    {
        private readonly ICommandService _commandService;

        public HandleEmployerWithdrawalCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleEmployerWithdrawalCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.EmployerWithdrawal)] EmployerWithdrawalCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
