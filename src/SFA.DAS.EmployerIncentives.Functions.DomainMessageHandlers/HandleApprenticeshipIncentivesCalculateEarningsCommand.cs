using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApprenticeshipIncentivesCalculateEarningsCommand
    {
        private readonly ICommandService _commandService;

        public HandleApprenticeshipIncentivesCalculateEarningsCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandleApprenticeshipIncentivesCalculateEarningsCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipIncentivesCalculateEarnings)] CalculateEarningsCommand command)
        {
             await _commandService.Dispatch(command);
        }
    }
}
