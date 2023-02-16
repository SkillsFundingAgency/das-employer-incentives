using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System;
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
            try
            {
                if (request.Type == JobType.RefreshLegalEntities)
                {
                    var command = ParseJobRequestToRefreshCommand(request);
                    await SendCommandAsync(command);
                }
                else if (request.Type == JobType.RefreshEmploymentChecks)
                {
                    var commands = ParseJobRequestToRefreshEmploymentCheckCommands(request);
                    await SendCommandsAsync(commands);
                }
            }
            catch (ArgumentException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return new BadRequestObjectResult(e.Message);
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

        private static List<RefreshEmploymentCheckCommand> ParseJobRequestToRefreshEmploymentCheckCommands(JobRequest request)
        {
            request.Data.TryGetValue("Requests", out object requests);
            var employmentCheckRefreshRequests = JsonConvert.DeserializeObject<IEnumerable<EmploymentCheckRefreshRequest>>(requests.ToString());
            
            var commands = new List<RefreshEmploymentCheckCommand>();
            foreach(var employmentCheckRefreshRequest in employmentCheckRefreshRequests)
            {
                foreach (var application in employmentCheckRefreshRequest.Applications)
                {
                    commands.Add(
                        new RefreshEmploymentCheckCommand(
                            employmentCheckRefreshRequest.CheckType,
                            application.AccountLegalEntityId,
                            application.ULN,
                            application.ServiceRequest?.TaskId ?? employmentCheckRefreshRequest.ServiceRequest.TaskId,
                            application.ServiceRequest?.DecisionReference ?? employmentCheckRefreshRequest.ServiceRequest.DecisionReference,
                            application.ServiceRequest?.TaskCreatedDate ?? employmentCheckRefreshRequest.ServiceRequest.TaskCreatedDate)
                    );
                }
            }

            return commands;
        }
    }
}