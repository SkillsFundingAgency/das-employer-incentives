using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System.Collections.Generic;
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

            var commands = new List<Task>();
            foreach (var apprenticeship in @event.EligibleApprenticeships())
            {
                var command = new CreateIncentiveCommand(
                    @event.Model.AccountId,
                    @event.Model.AccountLegalEntityId,
                    apprenticeship.Id,
                    apprenticeship.ApprenticeshipId,
                    apprenticeship.FirstName,
                    apprenticeship.LastName,
                    apprenticeship.DateOfBirth,
                    apprenticeship.ULN,
                    apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval,
                    apprenticeship.UKPRN,
                    @event.Model.DateSubmitted.Value,
                    @event.Model.SubmittedByEmail,
                    apprenticeship.CourseName,
                    apprenticeship.EmploymentStartDate.Value,
                    apprenticeship.Phase
                );

                var task = _commandPublisher.Publish(command);
                commands.Add(task);
            }

            return Task.WhenAll(commands);
        }
    }
}
