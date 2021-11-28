using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentCheckController : ApiCommandControllerBase
    {
        public EmploymentCheckController(ICommandDispatcher commandDispatcher) : base(commandDispatcher) { }

        [HttpPut("/employmentchecks/{correlationId}")]  
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update([FromBody] UpdateEmploymentCheckRequest updateRequest)
        {
            await SendCommandAsync(
                new UpdateEmploymentCheckCommand(
                    updateRequest.CorrelationId,
                    updateRequest.Result,
                    updateRequest.DateChecked)
                );

            return Ok($"/employmentchecks/{updateRequest.CorrelationId}");
        }
    }
}