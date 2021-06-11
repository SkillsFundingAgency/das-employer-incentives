using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks
{
    public class SendClawbacksCommandHandler : ICommandHandler<SendClawbacksCommand>
    {
        private readonly IPaymentDataRepository _paymentsRepository;
        private readonly IPaymentsQueryRepository _queryRepository;
        private readonly IBusinessCentralFinancePaymentsService _businessCentralFinancePaymentsService;

        public SendClawbacksCommandHandler(
            IPaymentDataRepository paymentsRepository,
            IPaymentsQueryRepository queryRepository,
            IBusinessCentralFinancePaymentsService businessCentralFinancePaymentsService)
        {
            _paymentsRepository = paymentsRepository;
            _queryRepository = queryRepository;
            _businessCentralFinancePaymentsService = businessCentralFinancePaymentsService;
        }

        public async Task Handle(SendClawbacksCommand command, CancellationToken cancellationToken = default)
        {
            var clawbacks = await _queryRepository.GetUnpaidClawbacks(command.AccountLegalEntityId);
            if (!clawbacks.Any())
            {
                return;
            }

            await Send(clawbacks, command.AccountLegalEntityId, command.ClawbackDate);
        }

        private async Task Send(List<PaymentDto> clawbacks, long accountLegalEntityId, DateTime clawbackDate, CancellationToken cancellationToken = default)
        {
            var clawbacksToSend = clawbacks.Take(_businessCentralFinancePaymentsService.PaymentRequestsLimit).ToList();
            if (!clawbacksToSend.Any())
            {
                return;
            }

            await _businessCentralFinancePaymentsService.SendPaymentRequests(clawbacksToSend);

            await _paymentsRepository.UpdateClawbackDates(clawbacksToSend.Select(s => s.PaymentId).ToList(), clawbackDate);

            if (clawbacks.Count > clawbacksToSend.Count)
            {
                await Send(clawbacks.Skip(_businessCentralFinancePaymentsService.PaymentRequestsLimit).ToList(), accountLegalEntityId, clawbackDate, cancellationToken);
            }
        }
    }
}
