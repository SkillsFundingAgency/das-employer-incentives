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
            if(learner is ILogWriter)
            {
                var learnerLog = (learner as ILogWriter).Log;

                try
                {
                    _logger.LogInformation($"Start refresh of learner data from learner match service : {learnerLog.OnProcessing?.Invoke()}");

                    await _learnerService.Refresh(learner);

                    _logger.LogInformation($"End refresh of learner data from learner match service : {learnerLog.OnProcessed?.Invoke()}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error refreshing learner data from learner match service : {learnerLog.OnError?.Invoke()}");

                    throw;
                }
            }
            else
            {
                await _learnerService.Refresh(learner);
            }
        }
    }
}
