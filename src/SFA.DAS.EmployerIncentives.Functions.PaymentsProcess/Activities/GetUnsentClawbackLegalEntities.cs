using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetUnsentClawbacks
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<GetPayableLegalEntities> _logger;

        public GetUnsentClawbacks(IQueryDispatcher queryDispatcher, ILogger<GetPayableLegalEntities> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(GetUnsentClawbacks))]
        public async Task<List<ClawbackLegalEntityDto>> Get([ActivityTrigger]CollectionPeriod collectionPeriod)
        {
            _logger.LogInformation("Getting unsent clawbacks for collection period {collectionPeriod}.", collectionPeriod);
			var clawbackLegalEntities = await _queryDispatcher.Send<GetClawbackLegalEntitiesRequest, GetClawbackLegalEntitiesResponse>(new GetClawbackLegalEntitiesRequest(collectionPeriod.Year, collectionPeriod.Period, false));
            _logger.LogInformation("{unsentClawbacksCount} legal entities with unsent clawbacks returned for collection period {collectionPeriod}.", clawbackLegalEntities.ClawbackLegalEntities.Count, collectionPeriod);

            return clawbackLegalEntities.ClawbackLegalEntities.ToList();
        }
    }
}
