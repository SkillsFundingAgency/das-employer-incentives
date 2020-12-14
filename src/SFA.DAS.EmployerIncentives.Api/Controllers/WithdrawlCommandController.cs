using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Withdrawls.EmployerWithdrawl;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class WithdrawlCommandController : ApiCommandControllerBase
    {
        public WithdrawlCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/withdrawls")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> WithdrawlIncentiveApplication([FromBody] WithdrawApplicationRequest request)
        {
            if (request.WithdrawlType == WithdrawlType.Employer)
            {
                await SendCommandAsync(
                    new EmployerWithdrawlCommand(
                        request.AccountLegalEntityId, 
                        request.ULN,
                        request.ServiceRequestTaskId,
                        request.ServiceRequestDecisionNumber,
                        request.ServiceRequestCreatedDate));
                return Accepted();
            }
            else
            {
                return BadRequest();
            }
        }       
    }
}