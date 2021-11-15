using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class CommandServiceWithLogging : ICommandService
    {
        private readonly ICommandService _commandService;
        private readonly ILoggerFactory _logfactory;

        public CommandServiceWithLogging(
            ICommandService commandService,
            ILoggerFactory logfactory)
        {
            _commandService = commandService;
            _logfactory = logfactory;
    }

        public async Task Dispatch<T>(T command) where T : DomainCommand
        {
            var log = _logfactory.CreateLogger<T>();

            try
            {
                log.LogDebug($"Start dispatch  '{typeof(T)}' domain command");

                await _commandService.Dispatch(command);

                log.LogDebug($"End dispatch '{typeof(T)}' domain command");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error dispatching '{typeof(T)}' domain command");

                throw;
            }
        }
    }
}
