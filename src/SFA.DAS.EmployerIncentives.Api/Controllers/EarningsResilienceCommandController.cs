using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("earnings-resilience-check")]
    [ApiController]
    public class EarningsResilienceCommandController : ApiCommandControllerBase
    {
        private readonly ICollectionCalendarService _collectionCalendarService;

        public EarningsResilienceCommandController(
            ICommandDispatcher commandDispatcher,
            ICollectionCalendarService collectionCalendarService) : base(commandDispatcher)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CheckApplications()
        {
            if (await ActivePeriodInProgress())
            {
                return Ok();
            }
            
            await SendCommandsAsync(new List<ICommand>() {
                    new IncompleteEarningsCalculationCheckCommand(),
                    new EarningsResilienceApplicationsCheckCommand(),
                    new EarningsResilienceIncentivesCheckCommand()
                });

            return Ok();
        }

        private async Task<bool> ActivePeriodInProgress()
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return activePeriod.PeriodEndInProgress;
        }
    }
}