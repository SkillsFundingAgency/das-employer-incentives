using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class LearningResumedHandler : IDomainEventHandler<LearningResumed>
    {
        private readonly IChangeOfCircumstancesDataRepository _repository;

        public LearningResumedHandler(IChangeOfCircumstancesDataRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(LearningResumed @event, CancellationToken cancellationToken = default)
        {
            var change = new Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance(
                Guid.NewGuid(),
                @event.ApprenticeshipIncentiveId,
                Enums.ChangeOfCircumstanceType.LearningStopped,
                true.ToString(),
                false.ToString(),
                @event.ResumedDate);

            return _repository.Save(change);
        }
    }
}
