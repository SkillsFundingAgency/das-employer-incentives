using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateDaysInLearning
{
    public class CalculateDaysInLearningCommandHandler : ICommandHandler<CalculateDaysInLearningCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public CalculateDaysInLearningCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(CalculateDaysInLearningCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);

            if(incentive == null)
            {
                return;
            }

            Learner learner = await _learnerDomainRepository.Get(incentive);
            if (learner.SubmissionData.SubmissionFound)
            {
                var calendar = await _collectionCalendarService.Get();
                var collectionPeriod = calendar.GetPeriod(command.CollectionYear, command.CollectionPeriodNumber);

                learner.SetDaysInLearning(collectionPeriod);
            }
            else
            {
                learner.ClearDaysInLearning();
            }

            await _learnerDomainRepository.Save(learner);
        }
    }
}
