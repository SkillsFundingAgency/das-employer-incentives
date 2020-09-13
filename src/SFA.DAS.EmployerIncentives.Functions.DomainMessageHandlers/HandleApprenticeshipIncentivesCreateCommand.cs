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

        [FunctionName("HandleApprenticeshipIncentivesCreateCommand")]
        public async Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipIncentivesCreate)] CreateCommand command)
        {
            await _commandDispatcher.Send(command);
        }
    }
}
