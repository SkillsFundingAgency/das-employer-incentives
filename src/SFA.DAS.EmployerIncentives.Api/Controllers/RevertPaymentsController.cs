using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class RevertPaymentsController : ApiCommandControllerBase
    {
        public RevertPaymentsController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/revert-payments")]
        public async Task<IActionResult> RevertPayments([FromBody] RevertPaymentsRequest request)
        {
            var commands = new List<ICommand>();

            try
            {
                request.Payments
                    .ToList()
                    .ForEach(v => commands.Add(
                        new RevertPaymentCommand(
                            v,
                            request.ServiceRequest.TaskId,
                            request.ServiceRequest.DecisionReference,
                            request.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow.Date)
                    ));

                await SendCommandsAsync(commands);

                return new OkObjectResult(new { Message = $"Payments have been successfully reverted" });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
