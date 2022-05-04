using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class ApplicationReinstatedHandler : IDomainEventHandler<ApplicationReinstated>
    {
        private readonly ICommandPublisher _commandPublisher;

        public ApplicationReinstatedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(ApplicationReinstated @event, CancellationToken cancellationToken = default)
        {
            return _commandPublisher.Publish(
                new ReinstateApprenticeshipIncentiveCommand(
                    @event.AccountId,
                    @event.Model.Id));
        }
    }
}
