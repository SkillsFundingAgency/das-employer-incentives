using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class LearnerServiceWithLogging : ILearnerService
    {
        private readonly ILearnerService _learnerService;
        private readonly ILoggerFactory _logfactory;

        public LearnerServiceWithLogging(
            ILearnerService learnerService,
            ILoggerFactory logfactory)
        {
            _learnerService = learnerService;
            _logfactory = logfactory;
        }

        public async Task Refresh(Learner learner)
        {
            var log = _logfactory.CreateLogger<Learner>();

            try
            {
                log.LogInformation($"Start refresh of learner data from learner match service for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId},  ApprenticeshipId : {learner.ApprenticeshipId}, Ukprn : {learner.Ukprn}, Url : {learner.UniqueLearnerNumber} " );

                await _learnerService.Refresh(learner);

                log.LogInformation($"Learner data refresh completed for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId},  ApprenticeshipId : {learner.ApprenticeshipId}, Ukprn : {learner.Ukprn}, Url : {learner.UniqueLearnerNumber} with result SubmissionFound : {learner.SubmissionFound} ");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error during learner data refresh for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId},  ApprenticeshipId : {learner.ApprenticeshipId}, Ukprn : {learner.Ukprn}, Url : {learner.UniqueLearnerNumber} with result SubmissionFound : {learner.SubmissionFound}");

                throw;
            }
        }
    }
}
