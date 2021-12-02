using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentCheckController : ApiCommandControllerBase
    {
        private readonly ICollectionCalendarService _collectionCalendarService;

        public EmploymentCheckController(ICommandDispatcher commandDispatcher, ICollectionCalendarService collectionCalendarService) 
            : base(commandDispatcher)
        {
            _collectionCalendarService = collectionCalendarService;
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

        [HttpPut("/employmentchecks/refresh")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Refresh()
        {
            if (await ActivePeriodInProgress())
            {
                return Ok();
            }

            await SendCommandAsync(new RefreshEmploymentChecksCommand());

            return Ok();
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

        private async Task<bool> ActivePeriodInProgress()
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return activePeriod.PeriodEndInProgress;
        }
    }
}