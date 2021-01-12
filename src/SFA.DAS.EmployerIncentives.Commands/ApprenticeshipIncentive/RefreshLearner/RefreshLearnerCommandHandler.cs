using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommandHandler : ICommandHandler<RefreshLearnerCommand>
    {
        private readonly ILogger<RefreshLearnerCommandHandler> _logger;
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerService _learnerService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;

        public RefreshLearnerCommandHandler(
            ILogger<RefreshLearnerCommandHandler> logger,
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerService learnerService,
            ILearnerDomainRepository learnerDomainRepository)
        {
            _logger = logger;
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerService = learnerService;
            _learnerDomainRepository = learnerDomainRepository;
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);
            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            _logger.LogInformation("Start Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN}",
                learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber);

            SubmissionData submissionData = null;
            var learnerData = await _learnerService.Get(learner);

            _logger.LogInformation("End Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN}",
                learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber);

            if (learnerData != null)
            {
                if (LearnerAndEarningsHaveNotChanged(learnerData, learner, incentive))
                {
                    return;
                }

                submissionData = new SubmissionData(learnerData.IlrSubmissionDate);
                submissionData.SetStartDate(learnerData.LearningStartDate(incentive));
                submissionData.SetLearningFound(learnerData.LearningFound(incentive));
                submissionData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));
                submissionData.SetIsInLearning(learnerData.IsInLearning(incentive));
                submissionData.SetRawJson(learnerData.RawJson);
            }

            if (submissionData != null && !submissionData.Equals(learner.SubmissionData))
            {
                incentive.SetHasPossibleChangeOfCircumstances(true);
            }

            learner.SetSubmissionData(submissionData);
            incentive.LearnerRefreshCompleted();

            learner.SetLearningPeriods(learnerData.LearningPeriods(incentive));


            if (learner.SubmissionData != null && !learner.SubmissionData.LearningFoundStatus.LearningFound)
            {
                _logger.LogInformation("Matching ILR record not found for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN} with reason: {NotFoundReason}",
                    learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber, learner.SubmissionData.LearningFoundStatus.NotFoundReason);
            }

            await _learnerDomainRepository.Save(learner);
            await _incentiveDomainRepository.Save(incentive);
        }

        private bool LearnerAndEarningsHaveNotChanged(LearnerSubmissionDto learnerData, Learner learner, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            return learnerData.IlrSubmissionDate == learner.SubmissionData?.SubmissionDate &&
                   incentive.RefreshedLearnerForEarnings;
        }
    }
}
