using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetPayableLegalEntities
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private ILogger<GetPayableLegalEntities> _logger;

        public GetPayableLegalEntities(IQueryDispatcher queryDispatcher, ILogger<GetPayableLegalEntities> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName("GetPayableLegalEntities")]
        public async Task<List<PayableLegalEntityDto>> Get([ActivityTrigger]CollectionPeriod collectionPeriod)
        {
            _logger.LogInformation("Getting payable legal entities for collection period {collectionPeriod}.", collectionPeriod);
            var legalEntities = await _queryDispatcher.Send<GetPayableLegalEntitiesRequest, GetPayableLegalEntitiesResponse>(new GetPayableLegalEntitiesRequest(collectionPeriod.Year, collectionPeriod.Month));
            var payableLegalEntitiesCount = legalEntities.PayableLegalEntities.Count;
            _logger.LogInformation("{payableLegalEntitiesCount} payable legal entities returned for collection period {collectionPeriod}.", payableLegalEntitiesCount, collectionPeriod);
            return legalEntities.PayableLegalEntities.ToList();
        }
    }
}
