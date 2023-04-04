using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
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

        [FunctionName(nameof(GetPayableLegalEntities))]
        public async Task<List<PayableLegalEntity>> Get([ActivityTrigger]CollectionPeriod collectionPeriod)
        {
			var legalEntities = await _queryDispatcher.Send<GetPayableLegalEntitiesRequest, GetPayableLegalEntitiesResponse>(new GetPayableLegalEntitiesRequest(new Domain.ValueObjects.CollectionPeriod(collectionPeriod.Period, collectionPeriod.Year)));
            
            return legalEntities.PayableLegalEntities.ToList();
        }
    }
}
