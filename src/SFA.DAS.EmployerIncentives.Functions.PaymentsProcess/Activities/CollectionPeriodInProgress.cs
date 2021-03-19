using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.CollectionPeriodInProgress;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CollectionPeriodInProgress
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<CollectionPeriodInProgress> _logger;

        public CollectionPeriodInProgress(IQueryDispatcher queryDispatcher, ILogger<CollectionPeriodInProgress> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(CollectionPeriodInProgress))]
        public async Task<bool> Get([ActivityTrigger] object input)
        {
            _logger.LogInformation("Getting Collection Period In Progress");
            var isInProgress = (await _queryDispatcher.Send<CollectionPeriodInProgressRequest, CollectionPeriodInProgressResponse>(new CollectionPeriodInProgressRequest())).IsInProgress;
            _logger.LogInformation($"Active collection period in progress : {isInProgress}");
            return isInProgress;
        }
    }
}
