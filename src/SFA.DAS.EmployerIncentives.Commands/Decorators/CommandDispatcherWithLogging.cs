using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class CommandDispatcherWithLogging : ICommandDispatcher
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly ILoggerFactory _logfactory;

        public CommandDispatcherWithLogging(
            ICommandDispatcher dispatcher,
            ILoggerFactory logfactory)
        {
            _dispatcher = dispatcher;
            _logfactory = logfactory;
        }

        public async Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            var log = _logfactory.CreateLogger<T>();
            var domainLog = (command is ILogWriter) ? (command as ILogWriter).Log : new Log();

            try
            {
                if (domainLog.OnProcessing == null)
                {
                    log.LogDebug($"Start dispatch  '{typeof(T)}' command");
                }
                else
                {
                    log.LogDebug($"Start dispatch '{typeof(T)}' command : {domainLog.OnProcessing.Invoke()}");
                }

                await _dispatcher.Send(command, cancellationToken);

                if (domainLog.OnProcessed == null)
                {
                    log.LogDebug($"End dispatch '{typeof(T)}' command");
                }
                else
                {
                    log.LogDebug($"End dispatch '{typeof(T)}' command : {domainLog.OnProcessed.Invoke()}");
                }
            }
            catch (Exception ex)
            {
                if (domainLog.OnError == null)
                {
                    log.LogError(ex, $"Error dispatching '{typeof(T)}' command");
                }
                else
                {
                    log.LogError(ex, $"Error dispatching '{typeof(T)}' command : {domainLog.OnError.Invoke()}");
                }

                throw;
            }
        }

        public Task SendMany<TCommands>(TCommands commands, CancellationToken cancellationToken = default) where TCommands : IEnumerable<ICommand>
        {
            return _dispatcher.SendMany(commands, cancellationToken);
        }
    }
}
