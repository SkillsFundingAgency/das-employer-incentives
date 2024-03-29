﻿using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLatestVendorRegistrationCaseUpdateDateTime;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus;

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

        [HttpGet("/accounts/vendorregistrationform/status")]
        public async Task<IActionResult> GetAccountsWithVrfCaseStatus(string vrfCaseStatus)
        {
            var request = new GetAccountsWithVrfCaseStatusRequest(vrfCaseStatus);

            var response = await QueryAsync<GetAccountsWithVrfCaseStatusRequest, GetAccountsWithVrfCaseStatusResponse>(request);

            return Ok(response.Accounts);
        }
                

        [HttpGet("/accounts/last-vrf-update-date")]
        public async Task<IActionResult> GetLatestVendorRegistrationCaseUpdateDateTime()
        {
            var request = new GetLatestVendorRegistrationCaseUpdateDateTimeRequest();
            var response = await QueryAsync<GetLatestVendorRegistrationCaseUpdateDateTimeRequest, GetLatestVendorRegistrationCaseUpdateDateTimeResponse>(request);

            return Ok(response);
        }
    }
}
