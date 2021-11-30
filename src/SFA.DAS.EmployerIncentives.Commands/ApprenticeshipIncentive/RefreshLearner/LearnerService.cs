using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    internal class LearnerService
    {
        private readonly ILearnerSubmissionService _learnerSubmissionService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        internal LearnerService(ILearnerSubmissionService learnerSubmissionService, ILearnerDomainRepository learnerDomainRepository, ICollectionCalendarService collectionCalendarService)
        {
            _learnerSubmissionService = learnerSubmissionService;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        internal async Task<Learner> Refresh(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            var learner = await _learnerDomainRepository.GetOrCreate(incentive);
            var learnerData = await _learnerSubmissionService.Get(learner);

            var collectionCalendar = await _collectionCalendarService.Get();

            var submissionDataService = new SubmissionDataService();
            learner.SetSubmissionData(submissionDataService.Get(learnerData, incentive, collectionCalendar));
            learner.SetLearningPeriods(learnerData.LearningPeriods(incentive, collectionCalendar));

            await _learnerDomainRepository.Save(learner);

            return learner;
        }
    }
}
