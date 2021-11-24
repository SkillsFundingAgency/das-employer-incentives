using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class GetAllApprenticeshipIncentives
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public GetAllApprenticeshipIncentives(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [FunctionName(nameof(GetAllApprenticeshipIncentives))]
        public async Task<List<ApprenticeshipIncentiveOutput>> Get([ActivityTrigger]object input)
        {
            var response = await _queryDispatcher.Send<GetApprenticeshipIncentivesRequest, GetApprenticeshipIncentivesResponse>(new GetApprenticeshipIncentivesRequest());
            return response.ApprenticeshipIncentives.Select(x=> new ApprenticeshipIncentiveOutput { Id = x.Id, ULN = x.ULN }).ToList();
        }
    }
}
