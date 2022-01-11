using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class CommandHandlerWithLogging<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly ILoggerFactory _logfactory;

        public CommandHandlerWithLogging(
            ICommandHandler<T> handler,
            ILoggerFactory logfactory)
        {
            _handler = handler;
            _logfactory = logfactory;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            var log = _logfactory.CreateLogger<T>();
            var domainLog = (command is ILogWriter) ? (command as ILogWriter).Log : new Log();

            try
            {
                if (domainLog.OnProcessing == null)
                {
                    log.LogDebug($"Start handle '{typeof(T)}' command");
                }
                else
                {
                    log.LogDebug($"Start handle '{typeof(T)}' command : {domainLog.OnProcessing.Invoke()}");
                }

                await _handler.Handle(command, cancellationToken);

                if (domainLog.OnProcessed == null)
                {
                    log.LogDebug($"End handle '{typeof(T)}' command");
                }
                else
                {
                    log.LogDebug($"End handle '{typeof(T)}' command : {domainLog.OnProcessed.Invoke()}");
                }
            }
            catch(Exception ex)
            {
                if (domainLog.OnError == null)
                {
                    log.LogError(ex, $"Error handling '{typeof(T)}' command");
                }
                else
                {
                    log.LogError(ex, $"Error handling '{typeof(T)}' command : {domainLog.OnError.Invoke()}");
                }

                throw;
            }
        }
    }
}
