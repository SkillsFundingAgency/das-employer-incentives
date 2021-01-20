using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApprenticeshipIncentivesCalculateEarningsCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleApprenticeshipIncentivesCalculateEarningsCommand(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("HandleApprenticeshipIncentivesCalculateEarningsCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipIncentivesCalculateEarnings)] CalculateEarningsCommand command)
        {
             await _commandDispatcher.Send(command);
        }
    }
}
