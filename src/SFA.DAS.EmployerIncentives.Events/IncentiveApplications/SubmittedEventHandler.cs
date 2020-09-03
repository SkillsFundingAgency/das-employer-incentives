using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class SubmittedEventHandler : IDomainEventHandler<Submitted>
    {
        private readonly ICommandPublisher _commandPublisher;

        public SubmittedEventHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(Submitted @event, CancellationToken cancellationToken = default)
        {
            var command = new CalculateClaimCommand(
                @event.AccountId,
                @event.IncentiveClaimApplicationId
                );

            return _commandPublisher.Publish(command);
        }
    }
}
