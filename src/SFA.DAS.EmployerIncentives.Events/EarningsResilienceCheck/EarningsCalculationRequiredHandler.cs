using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.EarningsResilienceCheck
{
    public class EarningsCalculationRequiredHandler : IDomainEventHandler<EarningsCalculationRequired>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EarningsCalculationRequiredHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EarningsCalculationRequired @event, CancellationToken cancellationToken = default)
        {
            var command = new CreateIncentiveCommand(
                @event.AccountId,
                @event.IncentiveApplicationId
                );

            return _commandPublisher.Publish(command);
        }
    }
}
