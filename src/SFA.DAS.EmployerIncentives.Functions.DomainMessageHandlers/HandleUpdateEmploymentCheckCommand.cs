using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleUpdateEmploymentCheckCommand
    {     
        private readonly ICommandService _commandService;

        public HandleUpdateEmploymentCheckCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandleUpdateEmploymentCheckCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.UpdateEmploymentCheck)] UpdateEmploymentCheckCommand command)
        {
           await _commandService.Dispatch(command);
        }
    }
}
