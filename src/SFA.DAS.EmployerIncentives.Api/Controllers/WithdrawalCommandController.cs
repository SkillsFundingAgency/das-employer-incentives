using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class WithdrawalCommandController : ApiCommandControllerBase
    {
        public WithdrawalCommandController(
            ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost("/withdrawals")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> WithdrawalIncentiveApplication([FromBody] WithdrawApplicationRequest request)
        {
            if (request.WithdrawalType == WithdrawalType.Employer)
            {
                var employerWithdrawalCommands = new List<EmployerWithdrawalCommand>();
                foreach(var application in request.Applications)
                {
                    employerWithdrawalCommands.Add(
                        new EmployerWithdrawalCommand(
                            application.AccountLegalEntityId,
                            application.ULN,
                            request.ServiceRequest.TaskId,
                            request.ServiceRequest.DecisionReference,
                            request.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow,
                            request.AccountId,
                            request.EmailAddress
                        ));
                }

                await SendCommandsAsync(employerWithdrawalCommands);
                return Accepted();
            }
            else if (request.WithdrawalType == WithdrawalType.Compliance)
            {
                var complianceWithdrawalCommands = new List<ComplianceWithdrawalCommand>();
                foreach (var application in request.Applications)
                {
                    complianceWithdrawalCommands.Add(
                        new ComplianceWithdrawalCommand(
                            application.AccountLegalEntityId,
                            application.ULN,
                            request.ServiceRequest.TaskId,
                            request.ServiceRequest.DecisionReference,
                            request.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow));
                }

                await SendCommandsAsync(complianceWithdrawalCommands);
                return Accepted();
            }
            else
            {
                return BadRequest(new { Error = "Invalid WithdrawalType of {request.WithdrawalType} passed in" });
            }
        }

        [HttpPost("/withdrawal-reinstatements")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ReinstateIncentiveApplication([FromBody] ReinstateApplicationRequest request)
        {
            var commands = new List<ICommand>();

            request.Applications
                .ToList()
                .ForEach(v => commands.Add(
                    new ReinstateWithdrawalCommand(
                        v.AccountLegalEntityId,
                        v.ULN)
                ));

            foreach (var command in commands)
            {
                await SendCommandAsync(command as dynamic);
            }

            return Accepted();
        }
    }
}