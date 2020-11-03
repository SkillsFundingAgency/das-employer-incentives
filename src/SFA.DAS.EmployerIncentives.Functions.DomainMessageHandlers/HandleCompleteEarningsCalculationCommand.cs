using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleCompleteEarningsCalculationCommand
    {     
        private readonly ICommandService _commandService;

        public HandleCompleteEarningsCalculationCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandleCompleteEarningsCalculationCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.CompleteEarningsCalculation)] CompleteEarningsCalculationCommand command)
        {
           await _commandService.Dispatch(command);
        }
    }
}
