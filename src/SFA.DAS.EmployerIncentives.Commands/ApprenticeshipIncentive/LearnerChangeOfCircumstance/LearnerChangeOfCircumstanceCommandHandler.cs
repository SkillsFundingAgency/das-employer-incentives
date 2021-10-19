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
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public LearnerChangeOfCircumstanceCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService,
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _domainRepository = domainRepository;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task Handle(LearnerChangeOfCircumstanceCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);            

            if (!incentive.HasPossibleChangeOfCircumstances)
            {
                return;
            }

            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            incentive.SetBreaksInLearning(learner);

            if (learner.HasFoundSubmission)
            {
                var collectionCalendar = await _collectionCalendarService.Get();
                var paymentProfiles = await _incentivePaymentProfilesService.Get();

                if (learner.HasStartDate)
                {
                    incentive.SetStartDateChangeOfCircumstance(learner.StartDate.Value, paymentProfiles, collectionCalendar);
                }

                incentive.SetLearningStoppedChangeOfCircumstance(learner.StoppedStatus, paymentProfiles, collectionCalendar);
            }

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
