using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{  
    public class SubmittedEventHandler : IDomainEventHandler<Submitted>
    {
        private readonly IEventPublisher _eventPublisher;

        public SubmittedEventHandler(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public Task Handle(Submitted @event, CancellationToken cancellationToken = default)
        {
            var submittedEvent = new EmployerIncentiveClaimSubmittedEvent
            {
                AccountId = @event.AccountId,
                IncentiveClaimApplicationId = @event.IncentiveClaimApplicationId
            };

            return _eventPublisher.Publish(submittedEvent); // this could publish a command
        }
    }
}
