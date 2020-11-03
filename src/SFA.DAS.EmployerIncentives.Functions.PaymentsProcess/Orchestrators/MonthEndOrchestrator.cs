using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class MonthEndOrchestrator
    {
        private readonly ILogger<MonthEndOrchestrator> _logger;
        private const string Orchestrator = nameof(MonthEndOrchestrator);

        public MonthEndOrchestrator(ILogger<MonthEndOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(Orchestrator)]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (!context.IsReplaying) _logger.LogInformation($"{Orchestrator} Started");

            var collectionPeriod = context.GetInput<CollectionPeriod>();

            await StartLearnerMatchingOrchestrator(context);
            await StartIncentivePaymentOrchestrator(context, collectionPeriod);

            _logger.LogInformation($"{Orchestrator} Completed");
        }

        private async Task StartLearnerMatchingOrchestrator(IDurableOrchestrationContext context)
        {
            const string subOrchestrator = nameof(LearnerMatchingOrchestrator);

            _logger.LogInformation($"Triggering {subOrchestrator}");

            await context.CallSubOrchestratorAsync(subOrchestrator, null);

            _logger.LogInformation($"Completed {subOrchestrator}");
        }

        private async Task StartIncentivePaymentOrchestrator(IDurableOrchestrationContext context, CollectionPeriod collectionPeriod)
        {
            const string subOrchestrator = nameof(IncentivePaymentOrchestrator);

            _logger.LogInformation($"Triggering {subOrchestrator} for collection period {collectionPeriod}", new { collectionPeriod });

            await context.CallSubOrchestratorAsync(subOrchestrator, collectionPeriod);

            _logger.LogInformation($"Completed {subOrchestrator} for collection period {collectionPeriod}", new { collectionPeriod });
        }
    }
}