using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
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

        public RefreshLearnerCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerService learnerService,
            ILearnerDomainRepository learnerDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerService = learnerService;
            _learnerDomainRepository = learnerDomainRepository;
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);

            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            SubmissionData submissionData = null;
            var learnerData = await _learnerService.Get(learner);
            if (learnerData != null)
            {
                submissionData = new SubmissionData(learnerData.IlrSubmissionDate);
                submissionData.SetStartDate(learnerData.LearningStartDate(incentive));
                submissionData.SetLearningFound(learnerData.LearningFound(incentive));
                submissionData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));
                submissionData.SetIsInLearning(learnerData.IsInLearning(incentive));
                submissionData.SetRawJson(learnerData.RawJson);
            }

            learner.SetSubmissionData(submissionData);

            await _learnerDomainRepository.Save(learner);
        }
    }
}
