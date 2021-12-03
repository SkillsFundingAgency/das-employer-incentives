using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Enums;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentCheckController : ApiCommandControllerBase
    {

        public EmploymentCheckController(ICommandDispatcher commandDispatcher) 
            : base(commandDispatcher)
        {
        }

        [HttpPut("/employmentchecks/{correlationId}")]  
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update([FromBody] UpdateEmploymentCheckRequest updateRequest)
        {
            await SendCommandAsync(
                new UpdateEmploymentCheckCommand(
                    updateRequest.CorrelationId,
                    Map(updateRequest.Result),
                    updateRequest.DateChecked)
                );

            return Ok($"/employmentchecks/{updateRequest.CorrelationId}");
        }

        private EmploymentCheckResultType Map(string result)
        {
            return result.ToLower() switch
            {
                "employed" => EmploymentCheckResultType.Employed,
                "notemployed" => EmploymentCheckResultType.NotEmployed,
                "hmrcunknown" => EmploymentCheckResultType.HMRCUnknown,
                "noninofound" => EmploymentCheckResultType.NoNINOFound,
                _ => EmploymentCheckResultType.NoAccountFound,
            };
        }

    }
}