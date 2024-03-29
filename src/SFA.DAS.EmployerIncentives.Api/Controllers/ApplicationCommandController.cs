﻿using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

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
        public async Task<IActionResult> SubmitIncentiveApplication([FromBody] SubmitIncentiveApplicationRequest request)
        {
            try
            {
                await SendCommandAsync(new SubmitIncentiveApplicationCommand(request.IncentiveApplicationId,
                    request.AccountId, request.DateSubmitted, request.SubmittedByEmail, request.SubmittedByName));
                return Ok();
            }
            catch (UlnAlreadySubmittedException)
            {
                return Conflict("Application contains a ULN which has already been submitted");
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}