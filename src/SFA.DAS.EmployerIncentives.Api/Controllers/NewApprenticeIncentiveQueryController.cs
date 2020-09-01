using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class NewApprenticeIncentiveQueryController : ApiQueryControllerBase
    {
        public NewApprenticeIncentiveQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        [HttpGet("/newapprenticeincentive")]
        public async Task<IActionResult> GetIncentiveDetails()
        {
            var request = new GetIncentiveDetailsRequest();
            var response = await QueryAsync<GetIncentiveDetailsRequest, GetIncentiveDetailsResponse>(request);

            return Ok(response);
        }
    }
}
