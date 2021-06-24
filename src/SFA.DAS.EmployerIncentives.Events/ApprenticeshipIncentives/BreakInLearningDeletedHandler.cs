using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class BreakInLearningDeletedHandler : IDomainEventHandler<BreakInLearningDeleted>
    {
        private readonly ILearnerDataRepository _repository;

        public BreakInLearningDeletedHandler(ILearnerDataRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(BreakInLearningDeleted @event, CancellationToken cancellationToken = default)
        {
            var learner = await _repository.GetByApprenticeshipIncentiveId(@event.ApprenticeshipIncentiveId);
            learner.SubmissionData.LearningData.StoppedStatus.Undo();

            await _repository.Update(learner);
        }
    }
}
