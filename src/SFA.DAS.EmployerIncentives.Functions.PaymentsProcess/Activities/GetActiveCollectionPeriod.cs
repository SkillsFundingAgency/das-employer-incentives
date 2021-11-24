using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetActiveCollectionPeriod
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public GetActiveCollectionPeriod(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [FunctionName(nameof(GetActiveCollectionPeriod))]
        public async Task<CollectionPeriod> Get([ActivityTrigger] object input)
        {
            var activePeriodDto = (await _queryDispatcher.Send<GetActiveCollectionPeriodRequest, GetActiveCollectionPeriodResponse>(new GetActiveCollectionPeriodRequest())).CollectionPeriod;
            return new CollectionPeriod() { Period = activePeriodDto.CollectionPeriodNumber, Year = activePeriodDto.CollectionYear, IsInProgress = activePeriodDto.IsInProgress };
        }
    }
}
