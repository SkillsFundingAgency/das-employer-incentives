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
                _log.LogDebug($"Start handle command '{typeof(T)}' : {domainLog.OnProcessing.Invoke()}");

                await _handler.Handle(command, cancellationToken);

                _log.LogDebug($"End handle command '{typeof(T)}' : {domainLog.OnProcessed.Invoke()}");
            }
            catch(Exception ex)
            {
                _log.LogError(ex, $"Error handling command '{typeof(T)}' : {domainLog.OnError.Invoke()}");

                throw;
            }
        }
    }
}
