using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLatestVendorRegistrationCaseUpdateDateTime;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus;
using SFA.DAS.EmployerIncentives.Api.Types;
using System;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountQueryController : ApiQueryControllerBase
    {
        public AccountQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        [HttpGet("/accounts/{accountId}/LegalEntities")]
        public async Task<IActionResult> GetLegalEntities(long accountId)
        {
            var request = new GetLegalEntitiesRequest(accountId);
            var response = await QueryAsync<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(request);

            if (response?.LegalEntities?.Count() > 0)
            {
                return Ok(response.LegalEntities);
            }

            return NotFound();
        }

        [HttpGet("/accounts/{accountId}/LegalEntities/{accountLegalEntityId}")]
        public async Task<IActionResult> GetLegalEntity(long accountId, long accountLegalEntityId)
        {
            var request = new GetLegalEntityRequest(accountId, accountLegalEntityId);
            var response = await QueryAsync<GetLegalEntityRequest, GetLegalEntityResponse>(request);

            if (response?.LegalEntity != null)
            {
                return Ok(response.LegalEntity);
            }

            return NotFound();
        }

        [HttpGet("/accounts/{accountId}/legalentity/{accountLegalEntityId}/applications")]
        public async Task<IActionResult> GetApplications(long accountId, long accountLegalEntityId)
        {
            var request = new GetApplicationsRequest(accountId, accountLegalEntityId);
            var response = await QueryAsync<GetApplicationsRequest, GetApplicationsResponse>(request);

            if (response?.ApprenticeApplications != null)
            {
                return Ok(response);
            }

            return NotFound();
        }

        [HttpGet("/accounts")]
        public async Task<IActionResult> GetAccounts(AccountFilter filter)
        {
            if(!string.IsNullOrEmpty(filter.VrfCaseStatus))
            {
                var request = new GetAccountsWithVrfCaseStatusRequest(filter.VrfCaseStatus);

                var response = await QueryAsync<GetAccountsWithVrfCaseStatusRequest, GetAccountsWithVrfCaseStatusResponse>(request);

                return Ok(response.Accounts);
            }

            if (!string.IsNullOrEmpty(filter.OrderBy) && 
                filter.OrderBy == "VrfCaseStatusLastUpdatedDateTime" && 
                filter.Limit.HasValue && 
                filter.Limit.Value == 1)
            {
                var request = new GetLatestVendorRegistrationCaseUpdateDateTimeRequest();
                var response = await QueryAsync<GetLatestVendorRegistrationCaseUpdateDateTimeRequest, GetLatestVendorRegistrationCaseUpdateDateTimeResponse>(request);
                
                return Ok(response); // ideally this would return an account dto as above but it has been implemented as RPC rather than as a resource                
            }
             
            return NotFound();
        }

        [HttpGet("/accounts/vendorregistrationform/status")]
        [Obsolete("Use accounts endpoint instead with vrfCaseStatus querystring")]
        public async Task<IActionResult> GetAccountsWithVrfCaseStatus(string vrfCaseStatus)
        {
            return await GetAccounts(new AccountFilter { VrfCaseStatus = vrfCaseStatus });
        }

        [HttpGet("/accounts/last-vrf-update-date")]
        [Obsolete("Use accounts endpoint instead with vrfCaseStatus querystring")]
        public async Task<IActionResult> GetLatestVendorRegistrationCaseUpdateDateTime()
        {
            return await GetAccounts(new AccountFilter { OrderBy = "VrfCaseStatusLastUpdatedDateTime", Limit = 1 });
        }
    }
}
