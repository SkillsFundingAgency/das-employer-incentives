using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("eligible-apprenticeships")]
    [ApiController]
    public class EligibleApprenticeshipsQueryController : ApiQueryControllerBase
    {
        private ILogger<EligibleApprenticeshipsQueryController> _logger;


        public EligibleApprenticeshipsQueryController(IQueryDispatcher queryDispatcher, ILogger<EligibleApprenticeshipsQueryController> logger) : base(queryDispatcher)
        {
            _logger = logger;
        }

        [HttpPost("{accountId}/{accountLegalEntityId}")]
        public async Task<IActionResult> AreApprenticeshipsEligible(long accountId, long accountLegalEntityId,
                                                                    [FromBody] IEnumerable<EligibleApprenticeshipCheckDetails> apprenticeshipDetails)
        {
            var results = new List<EligibleApprenticeshipResult>();

            foreach(var apprenticeshipDetail in apprenticeshipDetails)
            {
                var dto = new ApprenticeshipDto
                {
                    UniqueLearnerNumber = apprenticeshipDetail.Uln,
                    IsApproved = apprenticeshipDetail.IsApproved,
                    StartDate = apprenticeshipDetail.StartDate
                };
                var request = new GetApprenticeshipEligibilityRequest(dto);

                var response = await QueryAsync<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(request);

                results.Add(new EligibleApprenticeshipResult { Uln = apprenticeshipDetail.Uln, Eligible = response.IsEligible });
            }
            
            return Ok(results);
        }
    }

}
