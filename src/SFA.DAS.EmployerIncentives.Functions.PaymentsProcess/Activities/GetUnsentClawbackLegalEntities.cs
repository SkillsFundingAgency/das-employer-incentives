using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetUnsentClawbacks
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public GetUnsentClawbacks(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;        
        }

        [FunctionName(nameof(GetUnsentClawbacks))]
        public async Task<List<ClawbackLegalEntity>> Get([ActivityTrigger]CollectionPeriod collectionPeriod)
        {
			var clawbackLegalEntities = await _queryDispatcher.Send<GetClawbackLegalEntitiesRequest, GetClawbackLegalEntitiesResponse>(new GetClawbackLegalEntitiesRequest(new Domain.ValueObjects.CollectionPeriod(collectionPeriod.Period, collectionPeriod.Year), false));

            return clawbackLegalEntities.ClawbackLegalEntities.ToList();
        }
    }
}
