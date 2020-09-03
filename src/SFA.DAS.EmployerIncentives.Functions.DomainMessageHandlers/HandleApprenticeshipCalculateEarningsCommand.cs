using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Apprenticeship;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleApprenticeshipCalculateEarningsCommand
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<HandleApprenticeshipCalculateEarningsCommand> _logger;

        public HandleApprenticeshipCalculateEarningsCommand(ICommandDispatcher commandDispatcher, ILogger<HandleApprenticeshipCalculateEarningsCommand> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName("HandleApprenticeshipCalculateEarningsCommand")]
        public async Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.ApplicationCalculateClaim)] CalculateEarningsCommand command)
        {
            try
            {
                await _commandDispatcher.Send(command);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling Command Dispatcher from function 'HandleApprenticeshipCalculateEarningsCommand'", e);
                throw;
            }
        }
    }
}
