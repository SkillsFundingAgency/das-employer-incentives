using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleSendEmploymentCheckRequestsCommand
    {     
        private readonly ICommandService _commandService;

        public HandleSendEmploymentCheckRequestsCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandleSendEmploymentCheckRequestsCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.SendEmploymentCheckRequests)] SendEmploymentCheckRequestsCommand command)
        {
           await _commandService.Dispatch(command);
        }
    }
}
