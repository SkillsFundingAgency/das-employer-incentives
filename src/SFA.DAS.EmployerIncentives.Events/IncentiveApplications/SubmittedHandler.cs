using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System.Collections.Generic;
using System.Linq;
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
            var command = new CreateIncentiveCommand(
                @event.AccountId,
                @event.AccountLegalEntityId,
                MapForIncentive(@event.Apprenticeships)
                );

            return _commandPublisher.Publish(command);
        }

        private static List<CreateIncentiveCommand.IncentiveApprenticeship> MapForIncentive(IEnumerable<ApprenticeshipModel> apprenticeships)
        {
            return apprenticeships.Select(a => new CreateIncentiveCommand.IncentiveApprenticeship(
                a.Id,
                a.ApprenticeshipId,
                a.FirstName,
                a.LastName,
                a.DateOfBirth,
                a.Uln,
                a.ApprenticeshipEmployerTypeOnApproval,
                a.PlannedStartDate)
            ).ToList();
        }
    }
}
