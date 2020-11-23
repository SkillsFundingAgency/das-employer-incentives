using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class ChangeOfCircumstanceOrchestrator
    {
        private ILogger<ChangeOfCircumstanceOrchestrator> _logger;

        public ChangeOfCircumstanceOrchestrator(ILogger<ChangeOfCircumstanceOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName("ChangeOfCircumstanceOrchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var learnerChangeOfCircumstanceInput = context.GetInput<LearnerChangeOfCircumstanceInput>();

            if (!context.IsReplaying)
                _logger.LogInformation("Learner Change of Circumstances process started for apprenticeship Incentive {apprenticeshipIncentiveId}, Uln: {uln}", learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln);

            await context.CallActivityAsync("LearnerChangeOfCircumstanceActivity", learnerChangeOfCircumstanceInput);
            await context.CallActivityAsync("CalculateEarningsActivity", new CalculateEarningsInput(learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln));
            await context.CallActivityAsync("LearnerMatchAndUpdate", new LearnerMatchInput { ApprenticeshipIncentiveId = learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId });

            _logger.LogInformation("Learner Change of Circumstances process completed for apprenticeship Incentive {apprenticeshipIncentiveId}, Uln: {uln}", learnerChangeOfCircumstanceInput.ApprenticeshipIncentiveId, learnerChangeOfCircumstanceInput.Uln);
        }
    }
}
