using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class ApprenticeshipIncentiveCommandController : ApiCommandControllerBase
    {
        public ApprenticeshipIncentiveCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/earningsRecalculations")]
        public async Task<IActionResult> RecalculateEarnings([FromBody] RecalculateEarningsRequest request)
        {
            await SendCommandAsync(new RecalculateEarningsCommand(
                request.IncentiveLearnerIdentifiers.Select(dto => new IncentiveLearnerIdentifier(dto.AccountLegalEntityId, dto.ULN))));

            return NoContent();
        }
    }
}
