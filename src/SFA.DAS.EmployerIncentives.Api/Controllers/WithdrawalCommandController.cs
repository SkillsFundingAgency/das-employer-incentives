using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class WithdrawalCommandController : ApiCommandControllerBase
    {   

        public WithdrawalCommandController(
            ICommandDispatcher commandDispatcher) : base(commandDispatcher)
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
                        request.ServiceRequest.TaskCreatedDate??DateTime.UtcNow));
                return Accepted();
            }
            else if(request.WithdrawalType == WithdrawalType.Compliance)
            {
                await SendCommandAsync(
                    new ComplianceWithdrawalCommand(
                        request.AccountLegalEntityId,
                        request.ULN,
                        request.ServiceRequest.TaskId,
                        request.ServiceRequest.DecisionReference,
                        request.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow));
                return Accepted();
            }
            else
            {
                return BadRequest(new { Error = "Invalid WithdrawalType of {request.WithdrawalType} passed in" });
            }
        }       
    }
}