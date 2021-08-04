using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprenticeshipIncentiveQueryController : ApiQueryControllerBase
    {
        public ApprenticeshipIncentiveQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        [HttpGet("/accounts/{accountId}/legalEntities/{accountLegalEntityId}/apprenticeshipIncentives")]
        public async Task<IActionResult> GetApprenticeshipIncentives(long accountId, long accountLegalEntityId, bool includeWithdrawn = false)
        {
            var request = new GetApprenticeshipIncentivesForAccountLegalEntityRequest(accountId, accountLegalEntityId, includeWithdrawn);
            var response = await QueryAsync<GetApprenticeshipIncentivesForAccountLegalEntityRequest, GetApprenticeshipIncentivesForAccountLegalEntityResponse>(request);

            if (response?.ApprenticeshipIncentives?.Count() > 0)
            {
                return Ok(response.ApprenticeshipIncentives);
            }

            return NotFound();
        }
    }
}
