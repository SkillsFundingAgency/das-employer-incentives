using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("eligible-apprenticeships")]
    [ApiController]
    public class EligibleApprenticeshipsQueryController : ApiQueryControllerBase
    {
        public EligibleApprenticeshipsQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        // TODO : return an Apprenticeship with the IsEligible filter set
        // /apprenticeshipIncentives?isEligible={isEligible}
        // move to ApprenticeshipIncentiveQueryController
        [HttpGet("{uln}")]        
        public async Task<IActionResult> IsApprenticeshipEligible(long uln, [FromQuery]DateTime startDate, [FromQuery]bool isApproved)
        {
            var request = new GetApprenticeshipEligibilityRequest(new Apprenticeship { UniqueLearnerNumber = uln, StartDate = startDate, IsApproved = isApproved });
            var response = await QueryAsync<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(request);

            if (response.IsEligible)
            {
                return Ok();
            }

            return NotFound();
        }
    }
}
