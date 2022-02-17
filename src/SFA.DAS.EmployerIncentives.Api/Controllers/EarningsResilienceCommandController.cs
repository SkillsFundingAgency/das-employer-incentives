using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("earnings-resilience-check")]
    [ApiController]
    public class EarningsResilienceCommandController : ApiCommandControllerBase
    {
        public EarningsResilienceCommandController(
            ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("")]
        public async Task<IActionResult> CheckApplications()
        {
            await SendCommandAsync(new IncompleteEarningsCalculationCheckCommand());

            await SendCommandsAsync(new List<ICommand>() {
                new EarningsResilienceApplicationsCheckCommand(),
                new EarningsResilienceIncentivesCheckCommand()
            });

            return Ok();
        }
    }
}