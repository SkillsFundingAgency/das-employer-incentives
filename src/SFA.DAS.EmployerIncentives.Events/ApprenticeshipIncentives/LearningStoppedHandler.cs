using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class LearningStoppedHandler : IDomainEventHandler<LearningStopped>
    {
        private readonly IChangeOfCircumstancesDataRepository _repository;

        public LearningStoppedHandler(IChangeOfCircumstancesDataRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(LearningStopped @event, CancellationToken cancellationToken = default)
        {
            var change = new Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance(
                Guid.NewGuid(),
                @event.ApprenticeshipIncentiveId,
                Enums.ChangeOfCircumstanceType.LearningStopped,
                false.ToString(),
                true.ToString(),
                @event.StoppedDate);

            return _repository.Save(change);
        }
    }
}
