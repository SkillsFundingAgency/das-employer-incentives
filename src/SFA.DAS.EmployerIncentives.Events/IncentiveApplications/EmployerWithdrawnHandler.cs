using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class EmployerWithdrawnHandler :  IDomainEventHandler<EmployerWithdrawn>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EmployerWithdrawnHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EmployerWithdrawn @event, CancellationToken cancellationToken = default)
        {
            return _commandPublisher.Publish(
                new WithdrawCommand(
                    @event.AccountId,
                    @event.Model.Id,
                    WithdrawnBy.Employer));
        }
    }
}
