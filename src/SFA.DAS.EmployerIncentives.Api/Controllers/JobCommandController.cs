using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
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
            else if (request.Type == JobType.RefreshEmploymentChecks)
            {
                await SendCommandAsync(new RefreshEmploymentChecksCommand());
            }
            else if (request.Type == JobType.RefreshEmploymentCheck)
            {
                var command = ParseJobRequestToRefreshEmploymentCheckCommand(request);
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

        private static RefreshEmploymentCheckCommand ParseJobRequestToRefreshEmploymentCheckCommand(JobRequest request)
        {
            request.Data.TryGetValue("AccountLegalEntityId", out object accountLegalEntityIdRequest);
            request.Data.TryGetValue("ULN", out object ulnRequest);
            request.Data.TryGetValue("ServiceRequest", out object serviceRequestRequest);
            long.TryParse(accountLegalEntityIdRequest.ToString(), out long accountLegalEntityId);
            long.TryParse(ulnRequest.ToString(), out long uln);
            var serviceRequest = JsonConvert.DeserializeObject<ServiceRequest>(serviceRequestRequest.ToString());
            return new RefreshEmploymentCheckCommand(accountLegalEntityId, uln, serviceRequest.TaskId, serviceRequest.DecisionReference, serviceRequest.TaskCreatedDate);
        }
    }
}