using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApplicationCalculateClaimCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<HandleApplicationCalculateClaimCommand> _logger;

        public HandleApplicationCalculateClaimCommand(ICommandDispatcher commandDispatcher, ILogger<HandleApplicationCalculateClaimCommand> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName("HandleApplicationCalculateClaimCommand")]
        public async Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.ApplicationCalculateClaim)] CalculateClaimCommand command)
        {
            try
            {
                await _commandDispatcher.Send(command);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling Command Dispatcher", e);
                throw;
            }
        }
    }
}
