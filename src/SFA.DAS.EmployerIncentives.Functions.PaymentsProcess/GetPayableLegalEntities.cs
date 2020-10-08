using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetPayableLegalEntities
    {
        private readonly IQueryDispatcher _queryDispatcher;
        
        public GetPayableLegalEntities(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [FunctionName("GetPayableLegalEntities")]
        public async Task<List<long>> Get([ActivityTrigger]CollectionPeriod collectionPeriod, ILogger log)
        {
            var legalEntities = await _queryDispatcher.Send<GetPayableLegalEntitiesRequest, GetPayableLegalEntitiesResponse>(new GetPayableLegalEntitiesRequest(collectionPeriod.Year, collectionPeriod.Month));
            log.LogInformation($"{legalEntities.PayableLegalEntities.Count} payable legal entities returned.");
            return legalEntities.PayableLegalEntities.Select(x => x.AccountLegalEntityId).ToList();
        }
    }
}
