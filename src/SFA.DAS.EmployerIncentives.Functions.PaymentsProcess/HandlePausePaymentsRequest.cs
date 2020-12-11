using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class HandlePausePaymentsRequest
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandlePausePaymentsRequest(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("PausePaymentsRequest")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "accountlegalentity/{accountLegalEntityId}/ULN/{uln}/payments")] HttpRequest req, 
            long accountLegalEntityId,
            long uln,
            ILogger log)
        {
            try
            {
                log.LogInformation("Started Processing Pause Payment");
                var command = await CreateCommandFromRequest(req, accountLegalEntityId, uln, log);

                log.LogInformation("Calling Pause Payment Command Handler");
                await _commandDispatcher.Send(command);

                return new OkObjectResult(new { mesage = "Payments have been successfully paused"});
            }
            catch (InvalidRequestException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return new NotFoundObjectResult(new { e.Message });
            }
            catch (PausePaymentsException e)
            {
                return new BadRequestObjectResult(new { e.Message });
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e);
                return new InternalServerErrorResult();
            }
        }

        private async Task<PausePaymentsCommand> CreateCommandFromRequest(HttpRequest req, long accountLegalEntityId, long uln, ILogger log)
        {
            log.LogInformation("Creating Pause Payment Command");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var pauseRequest = JsonConvert.DeserializeObject<PausePaymentsRequest>(requestBody);

            return new PausePaymentsCommand(uln, accountLegalEntityId, pauseRequest.ServiceRequestId, pauseRequest.DecisionReferenceNumber, pauseRequest.DateServiceRequestTaskCreated);
        }
    }
}
