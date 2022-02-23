using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class ValidationOverrideController : ApiCommandControllerBase
    {
        public ValidationOverrideController(
            ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/validation-overrides")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Add([FromBody] ValidationOverrideRequest request)
        {
            var commands = new List<ICommand>();

            try
            {
                request.ValidationOverrides
                    .ToList()
                    .ForEach(v => commands.Add(
                        new ValidationOverrideCommand(
                            v.AccountLegalEntityId,
                            v.ULN,
                            v.ServiceRequest.TaskId,
                            v.ServiceRequest.DecisionReference,
                            v.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow.Date,
                            Map(v.ValidationSteps))
                        ));

                await SendCommandsAsync(commands);
            }
            catch(Exception)
            {
                return BadRequest();
            }            

            return Accepted();            
        } 
        
        private IEnumerable<ValidationOverrideStep> Map(ValidationStep[] validationStep)
        {
            return validationStep.Select(s => Map(s)).AsEnumerable();
        }

        private ValidationOverrideStep Map(ValidationStep validationStep)
        {
            return new ValidationOverrideStep(validationStep.ValidationType.ToString(), validationStep.ExpiryDate, validationStep.Remove ?? false);            
        }
    }
}