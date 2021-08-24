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

            if (learner.SubmissionData.SubmissionFound)
            {
                if (learner.SubmissionData.LearningData.StartDate.HasValue)
                {
                    incentive.SetStartDateChangeOfCircumstance(learner.SubmissionData.LearningData.StartDate.Value);

                    if (incentive.StartDateHasChanged())
                    {
                        await incentive.CalculateEarnings(_incentivePaymentProfilesService, _collectionCalendarService);
                    }
                }

                await incentive.SetLearningStoppedChangeOfCircumstance(learner.SubmissionData.LearningData.StoppedStatus, _collectionCalendarService);
            }

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
