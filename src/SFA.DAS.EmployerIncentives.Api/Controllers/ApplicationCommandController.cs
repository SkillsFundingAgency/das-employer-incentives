﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationCommandController : ApiCommandControllerBase
    {
        public ApplicationCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/applications")]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<IActionResult> CreateIncentiveApplication([FromBody]CreateIncentiveApplicationRequest request)
        {
            await SendCommandAsync(new CreateIncentiveApplicationCommand(request.IncentiveApplicationId, request.AccountId, request.AccountLegalEntityId, request.Apprenticeships));
            return Created($"/applications/{request.IncentiveApplicationId}", null);
        }

        [HttpPost("/submit-application")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public async Task<IActionResult> SubmitIncentiveApplication([FromBody] SubmitIncentiveApplicationRequest request)
        {
            try
            {
                await SendCommandAsync(new SubmitIncentiveApplicationCommand(request.IncentiveApplicationId, request.AccountId, request.DateSubmitted, request.SubmittedBy));
                return Ok();
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}