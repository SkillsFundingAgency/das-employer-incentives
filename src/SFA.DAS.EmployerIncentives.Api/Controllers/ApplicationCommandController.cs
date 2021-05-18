using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class ApplicationCommandController : ApiCommandControllerBase
    {
        public ApplicationCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/applications")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateIncentiveApplication([FromBody] CreateIncentiveApplicationRequest request)
        {
            await SendCommandAsync(new CreateIncentiveApplicationCommand(request.IncentiveApplicationId, request.AccountId, request.AccountLegalEntityId, request.Apprenticeships));
            return Created($"/applications/{request.IncentiveApplicationId}", null);
        }

        [HttpPut("/applications/{applicationId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateIncentiveApplication([FromBody] UpdateIncentiveApplicationRequest request)
        {
            await SendCommandAsync(new UpdateIncentiveApplicationCommand(request.IncentiveApplicationId, request.AccountId, request.Apprenticeships));
            return Ok($"/applications/{request.IncentiveApplicationId}");
        }

        [HttpPatch("/applications/{applicationId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SubmitIncentiveApplication([FromBody] PatchIncentiveApplicationRequest request)
        {
            try
            {
                switch (request.Action)
                {
                    case "RemoveIneligibleApprentices":
                    {
                        await SendCommandAsync(new RemoveIneligibleApprenticesFromApplicationCommand(request.IncentiveApplicationId, request.AccountId));
                        break;
                    }
                    default:
                    {
                        await SendCommandAsync(new SubmitIncentiveApplicationCommand(request.IncentiveApplicationId, request.AccountId, request.DateSubmitted, request.SubmittedByEmail, request.SubmittedByName));
                        break;
                    }
                }
                return Ok();
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}