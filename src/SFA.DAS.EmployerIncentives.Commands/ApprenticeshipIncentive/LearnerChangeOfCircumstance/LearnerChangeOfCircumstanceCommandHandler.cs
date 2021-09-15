using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class LearnerChangeOfCircumstanceCommandHandler : ICommandHandler<LearnerChangeOfCircumstanceCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public LearnerChangeOfCircumstanceCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _domainRepository = domainRepository;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(LearnerChangeOfCircumstanceCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);            

            if (!incentive.HasPossibleChangeOfCircumstances)
            {
                return;
            }

            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            if(learner.HasFoundSubmission)
            {
                var collectionCalendar = await _collectionCalendarService.Get();

                if (learner.HasStartDate)
                {
                    incentive.SetStartDateChangeOfCircumstance(learner.StartDate.Value, collectionCalendar);
                }

                incentive.SetLearningStoppedChangeOfCircumstance(learner.StoppedStatus, collectionCalendar);
            }

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
