using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("earnings-resilience-check")]
    [ApiController]
    public class EarningsResilienceCommandController : ApiCommandControllerBase
    { 
        public EarningsResilienceCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
          
        }

        [HttpPost("")]
        public async Task<IActionResult> CheckApplications()
        {
            await SendCommandAsync(new EarningsResilienceCheckCommand());

            return Ok();
        }
    }
}