using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class WithdrawalCommandController : ApiCommandControllerBase
    {
        public WithdrawalCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/withdrawals")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> WithdrawalIncentiveApplication([FromBody] WithdrawApplicationRequest request)
        {
            if (request.WithdrawalType == WithdrawalType.Employer)
            {
                await SendCommandAsync(
                    new EmployerWithdrawalCommand(
                        request.AccountLegalEntityId, 
                        request.ULN,
                        request.ServiceRequest.TaskId,
                        request.ServiceRequest.DecisionReference,
                        request.ServiceRequest.TaskCreatedDate??DateTime.Now));
                return Accepted();
            }
            else
            {
                return BadRequest();
            }
        }       
    }
}