using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections;
using System.Collections.Generic;
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
            if (request.Type == JobType.RefreshLegalEntities)
            {
                var command = ParseJobRequestToRefreshCommand(request);
                await SendCommandAsync(command);
            }
            return NoContent();
        }

        private static RefreshLegalEntitiesCommand ParseJobRequestToRefreshCommand(JobRequest request)
        {
            request.Data.TryGetValue("AccountLegalEntities", out object accountLegalEntitiesRequest);
            request.Data.TryGetValue("PageNumber", out object pageNumberRequest);
            request.Data.TryGetValue("PageSize", out object pageSizeRequest);
            request.Data.TryGetValue("TotalPages", out object totalPagesRequest);
            var accountLegalEntities = JsonConvert.DeserializeObject<IEnumerable<AccountLegalEntity>>(accountLegalEntitiesRequest.ToString());
            int.TryParse(pageNumberRequest.ToString(), out int pageNumber);
            int.TryParse(pageSizeRequest.ToString(), out int pageSize);
            int.TryParse(totalPagesRequest.ToString(), out int totalPages);
            var command = new RefreshLegalEntitiesCommand(accountLegalEntities, pageNumber, pageSize, totalPages);
            return command;
        }
    }
}