using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApprenticeshipIncentivesCreateCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleApprenticeshipIncentivesCreateCommand(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(HandleApprenticeshipIncentivesCreateCommand))]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipIncentivesCreate)] CreateIncentiveCommand command)
        {
            await _commandDispatcher.Send(command);
        }
    }
}
