using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class EarningsCalculatedHandler : IDomainEventHandler<EarningsCalculated>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EarningsCalculatedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EarningsCalculated @event, CancellationToken cancellationToken = default)
        {
            var command = new CompleteEarningsCalculationCommand(
                @event.AccountId,
                @event.ApplicationApprenticeshipId,
                @event.ApprenticeshipId,
                @event.ApprenticeshipIncentiveId);

            return _commandPublisher.Publish(command);
        }
    }
}
