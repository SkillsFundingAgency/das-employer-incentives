using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class EarningsRecalculationRequiredHandler : IDomainEventHandler<EarningsRecalculationRequired>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EarningsRecalculationRequiredHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EarningsRecalculationRequired @event, CancellationToken cancellationToken = default)
        {
            var command = new CalculateEarningsCommand(@event.ApprenticeshipIncentiveId);

            return _commandPublisher.Publish(command);
        }
    }
}
