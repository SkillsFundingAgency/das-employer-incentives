using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class StartDateChangedHandler : IDomainEventHandler<StartDateChanged>
    {
        private readonly IChangeOfCircumstancesDataRepository _repository;

        public StartDateChangedHandler(IChangeOfCircumstancesDataRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(StartDateChanged @event, CancellationToken cancellationToken = default)
        {
            var change = new Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance(
                Guid.NewGuid(),
                @event.ApprenticeshipIncentiveId,
                Enums.ChangeOfCircumstanceType.StartDate,
                @event.PreviousStartDate.ToString("yyyy-MM-dd"),
                @event.NewStartDate.ToString("yyyy-MM-dd"),
                DateTime.Now);

            return _repository.Save(change);
        }
    }
}
