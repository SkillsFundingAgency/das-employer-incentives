using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;
using System;
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
            try
            {
                await SendCommandAsync(
                    new UpdateEmploymentCheckCommand(
                        updateRequest.CorrelationId,
                        Map(updateRequest.Result),
                        updateRequest.DateChecked,
                        MapError(updateRequest.Result)
                        )
                    );

                return Ok($"/employmentchecks/{updateRequest.CorrelationId}");
            }
            catch (InvalidEmploymentCheckErrorTypeException)
            {
                return BadRequest();
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }
        }

        private EmploymentCheckResultType Map(string result)
        {
            return result.ToLower() switch
            {
                "employed" => EmploymentCheckResultType.Employed,
                "notemployed" => EmploymentCheckResultType.NotEmployed,
                _ => EmploymentCheckResultType.Error,
            };
        }

        private EmploymentCheckResultError? MapError(string errorResult)
        {  
            return errorResult.ToLower() switch
            {
                "employed" => null,
                "notemployed" => null,
                "ninonotfound" => EmploymentCheckResultError.NinoNotFound,
                "ninofailure" => EmploymentCheckResultError.NinoFailure,
                "ninoinvalid" => EmploymentCheckResultError.NinoInvalid,
                "payenotfound" => EmploymentCheckResultError.PAYENotFound,
                "payefailure" => EmploymentCheckResultError.PAYEFailure,
                "ninoandpayenotfound" => EmploymentCheckResultError.NinoAndPAYENotFound,
                "hmrcfailure" => EmploymentCheckResultError.HmrcFailure,
                _ => throw new InvalidOperationException("Invalid error result")
            };
        }

    }
}