using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    internal class LearnerMatchService
    {
        private readonly ILearnerService _learnerService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        internal LearnerMatchService(ILearnerService learnerService, ILearnerDomainRepository learnerDomainRepository, ICollectionCalendarService collectionCalendarService)
        {
            _learnerService = learnerService;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        internal async Task<Learner> RefreshLearner(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            var learner = await _learnerDomainRepository.GetOrCreate(incentive);
            var learnerData = await _learnerService.Get(learner);

            var collectionCalendar = await _collectionCalendarService.Get();

            var submissionDataService = new SubmissionDataService();
            learner.SetSubmissionData(submissionDataService.GetSubmissionData(learnerData, incentive, collectionCalendar));
            learner.SetLearningPeriods(learnerData.LearningPeriods(incentive, collectionCalendar));

            await _learnerDomainRepository.Save(learner);

            return learner;
        }
    }
}
