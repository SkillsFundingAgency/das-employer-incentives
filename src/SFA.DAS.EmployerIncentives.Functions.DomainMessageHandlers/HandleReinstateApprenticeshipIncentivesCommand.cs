using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleReinstateApprenticeshipIncentivesCommand
    {
        private readonly ICommandService _commandService;

        public HandleReinstateApprenticeshipIncentivesCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName(nameof(HandleReinstateApprenticeshipIncentivesCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipIncentivesReinstate)] ReinstateApprenticeshipIncentiveCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
