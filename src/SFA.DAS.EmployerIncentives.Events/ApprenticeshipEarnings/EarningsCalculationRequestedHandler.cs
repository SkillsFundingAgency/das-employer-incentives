using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.Apprenticeship;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipEarnings
{
    public class EarningsCalculationRequestedHandler : IDomainEventHandler<EarningsCalculationRequested>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EarningsCalculationRequestedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EarningsCalculationRequested @event, CancellationToken cancellationToken = default)
        {
            var command = new CalculateEarningsCommand(
                @event.AccountId,
                @event.IncentiveClaimApprenticeshipId,
                @event.ApprenticeshipId,
                @event.IncentiveType,
                @event.ApprenticeshipStartDate);

            return _commandPublisher.Publish(command);
        }
    }
}
