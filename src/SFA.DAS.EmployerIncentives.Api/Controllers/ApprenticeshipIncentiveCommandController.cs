using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class ApprenticeshipIncentiveCommandController : ApiCommandControllerBase
    {
        public ApprenticeshipIncentiveCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/recalculateEarnings")]
        public async Task<IActionResult> RecalculateEarnings([FromBody] RecalculateEarningsRequest request)
        {
            await SendCommandAsync(new RecalculateEarningsCommand(request.IncentiveLearnerIdentifiers));

            return NoContent();
        }
    }
}
