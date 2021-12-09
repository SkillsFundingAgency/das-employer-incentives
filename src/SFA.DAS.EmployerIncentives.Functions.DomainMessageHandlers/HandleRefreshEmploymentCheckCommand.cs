using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleRefreshEmploymentCheckCommand
    {     
        private readonly ICommandService _commandService;

        public HandleRefreshEmploymentCheckCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandleRefreshEmploymentCheckCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.RefreshEmploymentCheckCommand)] RefreshEmploymentCheckCommand command)
        {
           await _commandService.Dispatch(command);
        }
    }
}
