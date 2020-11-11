using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerServiceWithLogging : ILearnerService
    {
        private readonly ILearnerService _learnerService;
        private readonly ILogger<Learner> _logger;

        public LearnerServiceWithLogging(
            ILearnerService learnerService,
            ILogger<Learner> logger)
        {
            _learnerService = learnerService;
            _logger = logger;
        }

        public async Task Refresh(Learner learner)
        { 
            var learnerLog = (learner is ILogWriter) ? (learner as ILogWriter).Log : new Log();

            try
            {
                if (learnerLog.OnProcessing == null)
                {
                    _logger.LogInformation($"Start refresh of learner data from learner match service for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId}");
                }
                else
                {
                    _logger.LogInformation($"Start refresh of learner data from learner match service : {learnerLog.OnProcessing.Invoke()}");
                }

                await _learnerService.Refresh(learner);

                if (learnerLog.OnProcessed == null)
                {
                    _logger.LogInformation($"End refresh of learner data from learner match service for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId}");
                }
                else
                {
                    _logger.LogInformation($"End refresh of learner data from learner match service : {learnerLog.OnProcessed.Invoke()}");
                }
            }
            catch (Exception ex)
            {
                if (learnerLog.OnError == null)
                {
                    _logger.LogInformation($"Error refreshing learner data from learner match service for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId}");
                }
                else
                {
                    _logger.LogError(ex, $"Error refreshing learner data from learner match service : {learnerLog.OnError.Invoke()}");
                }

                throw;
            }

        }
    }
}
