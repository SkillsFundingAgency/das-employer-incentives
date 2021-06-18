using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetActiveCollectionPeriod
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<GetActiveCollectionPeriod> _logger;

        public GetActiveCollectionPeriod(IQueryDispatcher queryDispatcher, ILogger<GetActiveCollectionPeriod> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(GetActiveCollectionPeriod))]
        public async Task<CollectionPeriod> Get([ActivityTrigger] object input)
        {
            _logger.LogInformation("Getting active collection period");
            var activePeriodDto = (await _queryDispatcher.Send<GetActiveCollectionPeriodRequest, GetActiveCollectionPeriodResponse>(new GetActiveCollectionPeriodRequest())).CollectionPeriod;
            var activePeriod = new CollectionPeriod() { Period = activePeriodDto.CollectionPeriodNumber, Year = activePeriodDto.CollectionYear };
            _logger.LogInformation($"Active collection period number : {activePeriod.Period}, CollectionYear : {activePeriod.Year}");
            return activePeriod;
        }
    }
}
