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
        private readonly ILogger<T> _log;

        public CommandHandlerWithLogging(
            ICommandHandler<T> handler,         
            ILogger<T> log)
        {
            _handler = handler;
            _log = log;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            var domainLog = (command is ILogWriter) ? (command as ILogWriter).Log : new Log();

            try
            {
                _log.LogInformation($"Start handle '{typeof(T)}' command");
                if (domainLog.OnProcessing != null)
                {
                    _log.LogInformation(domainLog.OnProcessing.Invoke());
                }

                await _handler.Handle(command, cancellationToken);

                _log.LogInformation($"End handle '{typeof(T)}' command");
                if (domainLog.OnProcessed != null)
                {
                    _log.LogInformation(domainLog.OnProcessed.Invoke());
                }
            }
            catch(Exception ex)
            {    
                if (domainLog.OnError != null)
                {
                    _log.LogError(ex, $"Error handling '{typeof(T)}' command : {domainLog.OnError.Invoke()}");
                }
                else
                {
                    _log.LogError(ex, $"Error handling '{typeof(T)}' command");
                }

                throw;
            }
        }
    }
}
