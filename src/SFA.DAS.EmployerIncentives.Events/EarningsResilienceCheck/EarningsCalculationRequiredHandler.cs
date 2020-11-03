using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using System.Collections.Generic;
using System.Linq;
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
            var commands = new List<Task>();
            foreach (var apprenticeship in @event.Model.ApprenticeshipModels.Where(x => x.EarningsCalculated == false))
            {
                var command = new CreateApprenticeshipIncentiveCommand(
                    @event.Model.AccountId,
                    @event.Model.AccountLegalEntityId,
                    apprenticeship.Id,
                    apprenticeship.ApprenticeshipId,
                    apprenticeship.FirstName,
                    apprenticeship.LastName,
                    apprenticeship.DateOfBirth,
                    apprenticeship.Uln,
                    apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval
                );

                var task = _commandPublisher.Publish(command);
                commands.Add(task);
            }

            return Task.WhenAll(commands);
        }
    }
}
