using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Commands.Types.PausePayments;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandlePausePaymentsCommand
    {
        private readonly ICommandService _commandService;

        public HandlePausePaymentsCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [FunctionName("HandlePausePaymentsCommand")]
        public async Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.PausePaymentsCommand)] PausePaymentsCommand command)
        {
            await _commandService.Dispatch(command);
        }
    }
}
