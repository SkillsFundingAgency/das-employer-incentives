using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class PeriodEndOrchestrator
    {
        private readonly ILogger<PeriodEndOrchestrator> _logger;

        public PeriodEndOrchestrator(ILogger<PeriodEndOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(PeriodEndOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (!context.IsReplaying)
                _logger.LogInformation("PeriodEndOrchestrator Started");

            _logger.LogInformation("Calling Learner Matching Orchestrator");
            await context.CallSubOrchestratorAsync(nameof(LearnerMatchingOrchestrator), null);

            var paymentOrchestratorInstanceId = context.NewGuid().ToString();
            if(!context.IsReplaying)
                _logger.LogInformation("Calling Incentive Payment Orchestrator with instance id = {paymentOrchestratorInstanceId}", paymentOrchestratorInstanceId);
            await context.CallSubOrchestratorAsync(nameof(IncentivePaymentOrchestrator), paymentOrchestratorInstanceId, null);

            if (!context.IsReplaying)
                _logger.LogInformation("PeriodEndOrchestrator Completed");
        }
    }
}
