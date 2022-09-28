using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class ReinstatePaymentsController : ApiCommandControllerBase
    {
        public ReinstatePaymentsController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }
        
        [HttpPost("/reinstate-payments")]
        public async Task<IActionResult> ReinstatePayments([FromBody] ReinstatePaymentsRequest request)
        {
            var commands = new List<ICommand>();

            try
            {
                request.Payments
                    .ToList()
                    .ForEach(v => commands.Add(
                        new ReinstatePendingPaymentCommand(
                            v,
                            new ReinstatePaymentRequest(request.ServiceRequest.TaskId, 
                                                        request.ServiceRequest.DecisionReference, 
                                                        request.ServiceRequest.TaskCreatedDate, 
                                                        request.ServiceRequest.Process))
                    ));

                await SendCommandsAsync(commands);

                return new OkObjectResult(new { Message = $"Payments have been successfully reinstated" });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
