using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class BreakInLearningDeletedCoCHandler : IDomainEventHandler<BreakInLearningDeleted>
    {
        private readonly IChangeOfCircumstancesDataRepository _repository;

        public BreakInLearningDeletedCoCHandler(IChangeOfCircumstancesDataRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(BreakInLearningDeleted @event, CancellationToken cancellationToken = default)
        {
            var change = new Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance(
                Guid.NewGuid(),
                @event.ApprenticeshipIncentiveId,
                Enums.ChangeOfCircumstanceType.BreakInLearningDel,
                string.Empty,
                string.Empty,
                DateTime.Today);

            return _repository.Save(change);            
        }
    }
}
