using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.Decorators
{
    public class EventHandlerWithLogging<T> : IDomainEventHandler<T> where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler;
        private readonly ILogger<T> _log;

        public EventHandlerWithLogging(
            IDomainEventHandler<T> handler,
            ILogger<T> log)
        {
            _handler = handler;
            _log = log;
        }

        public async Task Handle(T @event, CancellationToken cancellationToken = default)
        {
            var domainLog = (@event is ILogWriter) ? (@event as ILogWriter).Log : new Log();

            try
            {
                if (domainLog.OnProcessing == null)
                {
                    _log.LogInformation($"Start handle '{typeof(T)}' event");
                }
                else
                {
                    _log.LogInformation($"Start handle '{typeof(T)}' event : {domainLog.OnProcessing.Invoke()}");
                }

                await _handler.Handle(@event, cancellationToken);


                if (domainLog.OnProcessed == null)
                {
                    _log.LogInformation($"End handle '{typeof(T)}' event");
                }
                else
                {
                    _log.LogInformation($"End handle '{typeof(T)}' event : {domainLog.OnProcessed.Invoke()}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error handling '{typeof(T)}' event");
                if (domainLog.OnError == null)
                {
                    _log.LogError(ex, $"Error handling '{typeof(T)}' event");
                }
                else
                {
                    _log.LogError(ex, $"Error handling '{typeof(T)}' event : {domainLog.OnError.Invoke()}");
                }
                throw;
            }
        }
    }
}
