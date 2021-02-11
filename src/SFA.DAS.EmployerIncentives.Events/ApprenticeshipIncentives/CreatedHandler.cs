using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class CreatedHandler : IDomainEventHandler<Created>
    {
        private readonly ICommandPublisher _commandPublisher;

        public CreatedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(Created @event, CancellationToken cancellationToken = default)
        {
            var command = new CalculateEarningsCommand(@event.ApprenticeshipIncentiveId);

            return _commandPublisher.Publish(command);
        }
    }
}
