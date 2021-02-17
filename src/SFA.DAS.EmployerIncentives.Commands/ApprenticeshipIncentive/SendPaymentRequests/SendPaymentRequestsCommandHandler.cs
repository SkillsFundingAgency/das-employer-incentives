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

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests
{
    public class SendPaymentRequestsCommandHandler : ICommandHandler<SendPaymentRequestsCommand>
    {
        private readonly IAccountDataRepository _accountRepository;
        private readonly IPayableLegalEntityQueryRepository _queryRepository;
        private readonly IBusinessCentralFinancePaymentsService _businessCentralFinancePaymentsService;

        public SendPaymentRequestsCommandHandler(
            IAccountDataRepository accountRepository,
            IPayableLegalEntityQueryRepository queryRepository,
            IBusinessCentralFinancePaymentsService businessCentralFinancePaymentsService)
        {
            _accountRepository = accountRepository;
            _queryRepository = queryRepository;
            _businessCentralFinancePaymentsService = businessCentralFinancePaymentsService;
        }

        public async Task Handle(SendPaymentRequestsCommand command, CancellationToken cancellationToken = default)
        {
            var payments = await _queryRepository.GetPaymentsToSendForAccountLegalEntity(command.AccountLegalEntityId);
            if (!payments.Any())
            {
                return;
            }

            await Send(payments, command.AccountLegalEntityId, command.PaidDate);
        }

        private async Task Send(List<PaymentDto> payments, long accountLegalEntityId, DateTime paidDate, CancellationToken cancellationToken = default)
        {
            var paymentsToSend = payments.Take(_businessCentralFinancePaymentsService.PaymentRequestsLimit).ToList();
            if (!paymentsToSend.Any())
            {
                return;
            }

            await _businessCentralFinancePaymentsService.SendPaymentRequests(paymentsToSend);

            await _accountRepository.UpdatePaidDateForPaymentIds(paymentsToSend.Select(s => s.PaymentId).ToList(), accountLegalEntityId, paidDate);

            if (payments.Count > paymentsToSend.Count)
            {
                await Send(payments.Skip(_businessCentralFinancePaymentsService.PaymentRequestsLimit).ToList(), accountLegalEntityId, paidDate, cancellationToken);
            }
        }
    }
}
