using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using System.Linq;
using System;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class PausePaymentsController : ApiCommandControllerBase
    {
        public PausePaymentsController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/pause-payments")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PausePayments([FromBody] PausePaymentsRequest request)
        {
            var commands = new List<ICommand>();

            try
            {
                request.Applications
                    .ToList()
                    .ForEach(v => commands.Add(
                        new PausePaymentsCommand(
                            v.ULN,
                            v.AccountLegalEntityId,                            
                            request.ServiceRequest.TaskId,
                            request.ServiceRequest.DecisionReference,
                            request.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow.Date,
                            request.Action)
                        ));

                await SendCommandsAsync(commands);
                return new OkObjectResult(new { Message = $"Payment {request.Action} Request(s) have been successfully queued" });
            }
            catch (InvalidRequestException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return new NotFoundObjectResult(new { e.Message });
            }
            catch (PausePaymentsException e)
            {
                return new BadRequestObjectResult(new { e.Message });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}