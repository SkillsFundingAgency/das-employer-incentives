using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApplicationCalculateClaimCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleApplicationCalculateClaimCommand(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("HandleEmployerIncentiveClaimSubmitted")]
        public Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.ApplicationCalculateClaim)] CalculateClaimCommand command)
        {
            return _commandDispatcher.Send(command);
        }
    }
}
