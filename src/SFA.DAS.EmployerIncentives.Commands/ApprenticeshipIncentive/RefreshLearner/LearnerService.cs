using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class LearnerService : ILearnerService
    {
        private readonly ILearnerSubmissionService _learnerSubmissionService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public LearnerService(
            ILearnerSubmissionService learnerSubmissionService, 
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _learnerSubmissionService = learnerSubmissionService;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task<Learner> Refresh(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
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
