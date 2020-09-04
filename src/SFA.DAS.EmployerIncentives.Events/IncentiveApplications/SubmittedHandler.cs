using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class SubmittedHandler : IDomainEventHandler<Submitted>
    {
        private readonly ICommandPublisher _commandPublisher;

        public SubmittedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(Submitted @event, CancellationToken cancellationToken = default)
        {
            var command = new CalculateClaimCommand(
                @event.AccountId,
                @event.IncentiveApplicationId
                );

            return _commandPublisher.Publish(command);
        }
    }
}
