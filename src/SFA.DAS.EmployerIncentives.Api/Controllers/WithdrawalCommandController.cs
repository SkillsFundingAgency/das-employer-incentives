using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class WithdrawalCommandController : ApiCommandControllerBase
    {
        private readonly IApprenticeshipIncentiveQueryRepository _queryRepository;

        public WithdrawalCommandController(
            ICommandDispatcher commandDispatcher,
            IApprenticeshipIncentiveQueryRepository queryRepository) : base(commandDispatcher)
        {
            _queryRepository = queryRepository;
        }

        [HttpPost("/withdrawals")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> WithdrawalIncentiveApplication([FromBody] WithdrawApplicationRequest request)
        {
            if(await HasPayments(request.AccountLegalEntityId, request.ULN))
            {
                return BadRequest(new { Error = "Cannot withdraw an application that has been submitted and has received payments" });
            }

            if (request.WithdrawalType == WithdrawalType.Employer)
            {
                await SendCommandAsync(
                    new EmployerWithdrawalCommand(
                        request.AccountLegalEntityId, 
                        request.ULN,
                        request.ServiceRequest.TaskId,
                        request.ServiceRequest.DecisionReference,
                        request.ServiceRequest.TaskCreatedDate??DateTime.UtcNow));
                return Accepted();
            }
            else if(request.WithdrawalType == WithdrawalType.Compliance)
            {
                await SendCommandAsync(
                    new ComplianceWithdrawalCommand(
                        request.AccountLegalEntityId,
                        request.ULN,
                        request.ServiceRequest.TaskId,
                        request.ServiceRequest.DecisionReference,
                        request.ServiceRequest.TaskCreatedDate ?? DateTime.UtcNow));
                return Accepted();
            }
            else
            {
                return BadRequest();
            }
        }       

        private async Task<bool> HasPayments(long accountLegalEntityId, long uln)
        {
            var incentiveModel = await _queryRepository.Get((a =>
               a.AccountLegalEntityId == accountLegalEntityId &&
               a.ULN == uln), includePayments: true);

            if (incentiveModel != null && incentiveModel.Payments.Any())
            {
                return true;
            }

            return false;
        }
    }
}