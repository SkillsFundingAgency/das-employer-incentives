using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommandHandler : ICommandHandler<RefreshLearnerCommand>
    {
        private readonly ILogger<RefreshLearnerCommandHandler> _logger;
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ILearnerService _learnerService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public RefreshLearnerCommandHandler(
            ILogger<RefreshLearnerCommandHandler> logger,
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            ILearnerService learnerService,
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _logger = logger;
            _incentiveDomainRepository = incentiveDomainRepository;
            _learnerService = learnerService;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(RefreshLearnerCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.Find(command.ApprenticeshipIncentiveId);
            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            var submissionData = new SubmissionData();
            var learnerData = await _learnerService.Get(learner);
            
            var collectionCalendar = await _collectionCalendarService.Get();

            if (learnerData != null)
            {
                if (LearnerAndEarningsHaveNotChanged(learnerData, learner, incentive))
                {
                    return;
                }

                submissionData.SetSubmissionDate(learnerData.IlrSubmissionDate);

                var learningFoundStatus = learnerData.LearningFound(incentive);
                submissionData.SetLearningData(new LearningData(learningFoundStatus.LearningFound,
                    learningFoundStatus.NotFoundReason));

                if (learningFoundStatus.LearningFound)
                {
                    submissionData.LearningData.SetStartDate(learnerData.LearningStartDate(incentive));
                    submissionData.LearningData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));
                    submissionData.LearningData.SetIsInLearning(learnerData.IsInLearning(incentive));
                    submissionData.LearningData.SetIsStopped(learnerData.IsStopped(incentive, collectionCalendar));
                }

                submissionData.SetRawJson(learnerData.RawJson);
            }

            ProcessLearningPeriods(learnerData, incentive, collectionCalendar, learner, submissionData);

            if (submissionData.HasChangeOfCircumstances(learner.SubmissionData))
            {
                incentive.SetHasPossibleChangeOfCircumstances(true);
            }

            learner.SetSubmissionData(submissionData);
            incentive.LearnerRefreshCompleted();

            if (!learner.SubmissionData.LearningData.LearningFound)
            {
                _logger.LogInformation("Matching ILR record not found for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN} with reason: {NotFoundReason}",
                    learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber, learner.SubmissionData.LearningData.NotFoundReason);
            }

            await _learnerDomainRepository.Save(learner);
            await _incentiveDomainRepository.Save(incentive);
        }

        private static void ProcessLearningPeriods(LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive,
            Domain.ValueObjects.CollectionCalendar collectionCalendar, Learner learner, SubmissionData submissionData)
        {
            var learningPeriods = learnerData.LearningPeriods(incentive, collectionCalendar);
            if (!LearningPeriodsChanged(learner.LearningPeriods, learningPeriods)) return;
            learner.SetLearningPeriods(learningPeriods);
            submissionData.LearningData.SetLearningPeriodsChanged();
        }

        private static bool LearningPeriodsChanged(IEnumerable<LearningPeriod> periods1, IEnumerable<LearningPeriod> periods2)
        {
            return !periods1.OrderBy(p => p.StartDate).SequenceEqual(periods2.OrderBy(p => p.StartDate));
        }

        private bool LearnerAndEarningsHaveNotChanged(LearnerSubmissionDto learnerData, Learner learner, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            //Commented out temporarily until the learner match api returns the correct date when an R13 is submitted after R01.
            //return learnerData.IlrSubmissionDate == learner.SubmissionData?.SubmissionDate && incentive.RefreshedLearnerForEarnings;
            return false;
        }
    }
}
