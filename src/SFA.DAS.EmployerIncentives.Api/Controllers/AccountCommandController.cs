using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountCommandController : ApiCommandControllerBase
    {
        public AccountCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher) { }

        [HttpPost("/accounts/{accountId}/legalEntities")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> AddLegalEntity([FromRoute] long accountId, [FromBody] AddLegalEntityRequest request)
        {
            await SendCommandAsync(new AddLegalEntityCommand(accountId, request.LegalEntityId, request.OrganisationName, request.AccountLegalEntityId));
            return Created($"/accounts/{accountId}/LegalEntities", new LegalEntityDto { AccountId = accountId, AccountLegalEntityId = request.AccountLegalEntityId, LegalEntityId = request.LegalEntityId, LegalEntityName = request.OrganisationName });
        }

        [HttpDelete("/accounts/{accountId}/legalEntities/{accountLegalEntityId}")]
        public async Task<IActionResult> RemoveLegalEntity([FromRoute] long accountId, long accountLegalEntityId)
        {
            await SendCommandAsync(new RemoveLegalEntityCommand(accountId, accountLegalEntityId));
            return NoContent();
        }

        [HttpPatch("/accounts/{accountId}/legalEntities/{accountLegalEntityId}")]
        public async Task<IActionResult> SignAgreement([FromRoute] long accountId, [FromRoute] long accountLegalEntityId, [FromBody] SignAgreementRequest request)
        {
            await SendCommandAsync(new SignLegalEntityAgreementCommand(accountId, accountLegalEntityId, request.AgreementVersion));
            return NoContent();
        }
    }
}
