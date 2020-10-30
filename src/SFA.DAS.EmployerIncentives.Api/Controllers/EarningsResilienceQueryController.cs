using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("earnings-resilience-check")]
    [ApiController]
    public class EarningsResilienceQueryController : ApiQueryControllerBase
    {
        public EarningsResilienceQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
          
        }

        [HttpGet("")]
        public async Task<IActionResult> CheckApplications()
        {
            var response = await QueryAsync<EarningsResilienceCheckRequest, EarningsResilienceCheckResponse>(new EarningsResilienceCheckRequest());

            if (response == null || response.Applications.Count() == 0)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}