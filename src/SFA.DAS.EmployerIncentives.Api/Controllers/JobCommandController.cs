using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobCommandController : ApiCommandControllerBase
    {
        public JobCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher) { }

        [HttpPut("/jobs")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddJob([FromBody] JobRequest request)
        {
            if(request.Type == JobType.RefreshLegalEntities)
            {
                request.Data.TryGetValue("PageNumber", out object pageNumberRequest);
                request.Data.TryGetValue("PageSize", out object pageSizeRequest);
                int.TryParse(pageNumberRequest?.ToString() ?? "1", out int pageNumber);
                int.TryParse(pageSizeRequest?.ToString()?? "500", out int pageSize);

                await SendCommandAsync(new RefreshLegalEntitiesCommand(pageNumber, pageSize));
            } 

            return NoContent();
        }
    }
}
