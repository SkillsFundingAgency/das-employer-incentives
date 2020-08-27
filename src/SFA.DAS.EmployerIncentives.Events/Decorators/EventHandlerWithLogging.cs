using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
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
            try
            {
                _log.LogInformation($"Start handle '{typeof(T)}' event");
                await _handler.Handle(@event, cancellationToken);
                _log.LogInformation($"End handle '{typeof(T)}' event");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error handling '{typeof(T)}' event");
                throw;
            }
        }
    }
}
