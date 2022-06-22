using System;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class ApplicationQueryController : ApiQueryControllerBase
    {
        public ApplicationQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        [HttpGet("/accounts/{accountId}/applications/{applicationId}")]
        public async Task<IActionResult> GetApplication(long accountId, Guid applicationId)
        {
            var request = new GetApplicationRequest(accountId, applicationId);
            var response = await QueryAsync<GetApplicationRequest, GetApplicationResponse>(request);

            if (response?.Application != null)
            {
                return Ok(response.Application);
            }

            return NotFound();
        }

        [HttpGet("/accounts/{accountId}/applications/{applicationId}/accountlegalentity")]
        [Obsolete("Use the applications GET endpoint and get the account legal entity id from the application returned")]
        public async Task<IActionResult> GetApplicationAccountLegalEntity(long accountId, Guid applicationId)
        {
            var request = new GetApplicationRequest(accountId, applicationId);
            var response = await QueryAsync<GetApplicationRequest, GetApplicationResponse>(request);

            if (response?.Application != null)
            {
                return Ok(response.Application.AccountLegalEntityId);
            }

            return NotFound();
        }
    }
}
