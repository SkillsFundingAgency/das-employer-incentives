using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class EmploymentChecksCreatedHandler : IDomainEventHandler<EmploymentChecksCreated>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EmploymentChecksCreatedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EmploymentChecksCreated @event, CancellationToken cancellationToken = default)
        {
            var command = new SendEmploymentCheckRequestsCommand(@event.ApprenticeshipIncentiveId);

            return _commandPublisher.Publish(command);
        }
    }
}
