using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class LearningStoppedHandler : IDomainEventHandler<LearningStopped>
    {
        private readonly IChangeOfCircumstancesDataRepository _repository;

        public LearningStoppedHandler(IChangeOfCircumstancesDataRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(LearningStopped @event, CancellationToken cancellationToken = default)
        {
            var previousLearningStoppedDate = string.Empty;
            var changeOfCircumstances = await _repository.GetList(x => x.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId);
            
            // LINQ expression does not match the enum values so have to filter separately
            if (changeOfCircumstances.Any(y => y.Type == ChangeOfCircumstanceType.LearningStopped))
            {
                previousLearningStoppedDate = changeOfCircumstances.Where(x => x.Type == ChangeOfCircumstanceType.LearningStopped)
                    .OrderByDescending(x => x.ChangedDate).First().NewValue;
            }

            var change = new Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance(
                Guid.NewGuid(),
                @event.ApprenticeshipIncentiveId,
                Enums.ChangeOfCircumstanceType.LearningStopped,
                previousLearningStoppedDate,
                @event.StoppedDate.ToString("yyyy-MM-dd"),
                DateTime.Today);

            await _repository.Save(change);
        }
    }
}
