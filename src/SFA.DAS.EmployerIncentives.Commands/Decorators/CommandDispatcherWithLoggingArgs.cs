using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class CommandDispatcherWithLoggingArgs : ICommandDispatcher
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly ILoggerFactory _logfactory;

        public CommandDispatcherWithLoggingArgs(
            ICommandDispatcher dispatcher,
            ILoggerFactory logfactory)
        {
            _dispatcher = dispatcher;
            _logfactory = logfactory;
        }

        public async Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            if (command is ILogWriterWithArgs)
            {

                var log = _logfactory.CreateLogger<T>();
                var domainLog = (command is ILogWriterWithArgs) ? (command as ILogWriterWithArgs).Log : new LogWithArgs();

                try
                {
                    var processing = domainLog.OnProcessing.Invoke();

                    log.LogDebug($"Start dispatch '{typeof(T)}' command : {processing.Item1}", processing.Item2);

                    await _dispatcher.Send(command, cancellationToken);

                    var processed = domainLog.OnProcessed.Invoke();

                    log.LogDebug($"End dispatch '{typeof(T)}' command : {processed.Item1}", processed.Item2);
                }
                catch (Exception ex)
                {
                    var errored = domainLog.OnError.Invoke();

                    log.LogError(ex, $"Error dispatching '{typeof(T)}' command : {errored.Item1}", errored.Item2);

                    throw;
                }
            }
            else
            {
                await _dispatcher.Send(command, cancellationToken);
            }
            
        }

        public Task SendMany<TCommands>(TCommands commands, CancellationToken cancellationToken = default) where TCommands : IEnumerable<ICommand>
        {
            return _dispatcher.SendMany(commands, cancellationToken);
        }
    }
}
