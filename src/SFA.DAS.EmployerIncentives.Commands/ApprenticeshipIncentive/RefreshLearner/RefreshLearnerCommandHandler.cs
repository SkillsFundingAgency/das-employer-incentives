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

            SubmissionData submissionData = new SubmissionData();
            var learnerData = await _learnerService.Get(learner);

            _logger.LogInformation("End Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN}",
                learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber);

            if (learnerData != null)
            {
                submissionData.SetSubmissionDate(learnerData.IlrSubmissionDate);

                var learningFoundStatus = learnerData.LearningFound(incentive);
                submissionData.SetLearningData(new LearningData(learningFoundStatus.LearningFound, learningFoundStatus.NotFoundReason));

                if (learningFoundStatus.LearningFound)
                {
                    submissionData.LearningData.SetStartDate(learnerData.LearningStartDate(incentive));
                    submissionData.LearningData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));
                    submissionData.LearningData.SetIsInLearning(learnerData.IsInLearning(incentive));
                }
                submissionData.SetRawJson(learnerData.RawJson);
            }

            if (!submissionData.HasChangeOfCircumstances(learner.SubmissionData))
            {
                incentive.SetHasPossibleChangeOfCircumstances(true);
            }

            learner.SetSubmissionData(submissionData);
            incentive.LearnerRefreshCompleted();

            learner.SetLearningPeriods(learnerData.LearningPeriods(incentive));

            if (!learner.SubmissionData.LearningData.LearningFound)
            {
                _logger.LogInformation("Matching ILR record not found for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN} with reason: {NotFoundReason}",
                    learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber, learner.SubmissionData.LearningData.NotFoundReason);
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
