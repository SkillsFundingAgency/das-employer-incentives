using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.AddEmployerVendorIdForLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class AccountCommandController : ApiCommandControllerBase
    {
        public AccountCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher) { }

        [HttpPut("/accounts/{accountId}/legalEntities")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> UpsertLegalEntity([FromRoute] long accountId, [FromBody] AddLegalEntityRequest request)
        {
            await SendCommandAsync(new UpsertLegalEntityCommand(accountId, request.LegalEntityId, request.OrganisationName, request.AccountLegalEntityId));
            return Created($"/accounts/{accountId}/LegalEntities", new LegalEntity { AccountId = accountId, AccountLegalEntityId = request.AccountLegalEntityId, LegalEntityId = request.LegalEntityId, LegalEntityName = request.OrganisationName });
        }

        [HttpDelete("/accounts/{accountId}/legalEntities/{accountLegalEntityId}")]
        public async Task<IActionResult> RemoveLegalEntity([FromRoute] long accountId, long accountLegalEntityId)
        {
            await SendCommandAsync(new RemoveLegalEntityCommand(accountId, accountLegalEntityId));
            return NoContent();
        }

        [HttpPost("/signedAgreements")]
        public async Task<IActionResult> SignedAgreements([FromBody] SignedAgreement request)
        {
            await SendCommandAsync(new SignLegalEntityAgreementCommand(
                request.AccountId,
                request.AccountLegalEntityId,
                request.AgreementVersion,
                request.LegalEntityName,
                request.LegalEntityId
                ));
            return NoContent();
        }

        [HttpPatch("/accounts/{accountId}/legalEntities/{accountLegalEntityId}")]
        [Obsolete("Use SignedAgreements endpoint to sign the agreement")]
        public async Task<IActionResult> SignAgreement([FromRoute] long accountId, [FromRoute] long accountLegalEntityId, [FromBody] SignedAgreement request)
        {
            request.AccountId = accountId;
            request.AccountLegalEntityId = accountLegalEntityId;
            return await SignedAgreements(request);
        }

        [HttpPost("/changeOfVendorCases")]
        public async Task<IActionResult> ChangeOfVendorCases([FromBody] ChangeOfVendorCase request)
        {
            await SendCommandAsync(new UpdateVendorRegistrationCaseStatusCommand(request.HashedLegalEntityId, request.CaseId, request.Status, request.CaseStatusLastUpdatedDate));
            return NoContent();
        }

        [HttpPatch("/legalentities/{hashedLegalEntityId}/vendorregistrationform")]
        [Obsolete("Use changeOfVendorCases endpoint instead")]
        public async Task<IActionResult> UpdateVendorRegistrationCaseStatus([FromRoute] string hashedLegalEntityId, [FromBody] ChangeOfVendorCase request)
        {
            request.HashedLegalEntityId = hashedLegalEntityId;
            return await ChangeOfVendorCases(request);
        }

        [HttpPost("/vendors")]
        public async Task<IActionResult> Vendors([FromBody] Vendors request)
        {
            await SendCommandAsync(new AddEmployerVendorIdForLegalEntityCommand(request.HashedLegalEntityId, request.EmployerVendorId));
            return NoContent();
        }

        [HttpPut("/legalentities/{hashedLegalEntityId}/employervendorid")]
        [Obsolete("Use legalentities/{hashedLegalEntityId}/vendors endpoint instead")]
        public async Task<IActionResult> AddEmployerVendorId([FromRoute] string hashedLegalEntityId, [FromBody] Vendors request)
        {
            request.HashedLegalEntityId = hashedLegalEntityId;
            return await Vendors(request);
        }
    }
}