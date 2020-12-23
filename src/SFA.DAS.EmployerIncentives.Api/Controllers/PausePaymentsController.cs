using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class PausePaymentsController : ApiCommandControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public PausePaymentsController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost("/pause-payments")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PausePayments([FromBody] PausePaymentsRequest request)
        {
            try
            {
                await _commandDispatcher.Send(new PausePaymentsCommand(request.ULN, request.AccountLegalEntityId, request.ServiceRequest?.TaskId, request.ServiceRequest?.DecisionReference, request.ServiceRequest?.TaskCreatedDate, request.Action));

                return new OkObjectResult(new { Message = $"Payments have been successfully {request.Action}d" });
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
        }
    }
}