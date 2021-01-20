using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApprenticeshipIncentivesWithdrawCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleApprenticeshipIncentivesWithdrawCommand(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(HandleApprenticeshipIncentivesWithdrawCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipIncentivesWithdraw)] WithdrawCommand command)
        {
            await _commandDispatcher.Send(command);
        }
    }
}
