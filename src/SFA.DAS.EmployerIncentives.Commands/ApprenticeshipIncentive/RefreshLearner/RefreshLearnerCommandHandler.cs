using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommandHandler : ICommandHandler<RefreshLearnerCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerService _learnerService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public RefreshLearnerCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerService learnerService,
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerService = learnerService;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);

            Learner learner = await _learnerDomainRepository.GetOrCreate(incentive);

            SubmissionData submissionData = null;
            var learnerData = await _learnerService.Get(learner);
            if (learnerData != null)
            {
                submissionData = new SubmissionData(learnerData.IlrSubmissionDate);
                submissionData.SetStartDate(learnerData.LearningStartDate(incentive));
                submissionData.SetLearningFound(learnerData.LearningFound(incentive));
                submissionData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));
                submissionData.SetIsInLearning(learnerData.IsInLearning(incentive));                
                submissionData.SetDaysInLearning(learnerData.DaysInLearning(incentive, await _collectionCalendarService.Get()));
                submissionData.SetRawJson(learnerData.RawJson);
            }

            learner.SetSubmissionData(submissionData);

            await _learnerDomainRepository.Save(learner);
        }
    }
}
