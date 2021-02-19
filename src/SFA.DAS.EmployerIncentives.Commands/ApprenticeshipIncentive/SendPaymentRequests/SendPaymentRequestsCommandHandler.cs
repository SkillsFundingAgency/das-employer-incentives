using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests
{
    public class SendPaymentRequestsCommandHandler : ICommandHandler<SendPaymentRequestsCommand>
    {
        private readonly IAccountDataRepository _accountRepository;
        private readonly IPaymentsQueryRepository _queryRepository;
        private readonly IBusinessCentralFinancePaymentsService _businessCentralFinancePaymentsService;

        public SendPaymentRequestsCommandHandler(
            IAccountDataRepository accountRepository,
            IPaymentsQueryRepository queryRepository,
            IBusinessCentralFinancePaymentsService businessCentralFinancePaymentsService)
        {
            _accountRepository = accountRepository;
            _queryRepository = queryRepository;
            _businessCentralFinancePaymentsService = businessCentralFinancePaymentsService;
        }

        public async Task Handle(SendPaymentRequestsCommand command, CancellationToken cancellationToken = default)
        {
            var payments = await _queryRepository.GetUnpaidPayments(command.AccountLegalEntityId);
            if (payments.Any() == false)
            {
                return;
            }

            var paymentsToSend = payments.Take(_businessCentralFinancePaymentsService.PaymentRequestsLimit).ToList();

            await _businessCentralFinancePaymentsService.SendPaymentRequests(paymentsToSend);

            await _accountRepository.UpdatePaidDateForPaymentIds(paymentsToSend.Select(s => s.PaymentId).ToList(), command.AccountLegalEntityId, command.PaidDate);

            if (payments.Count > paymentsToSend.Count)
            {
                await Handle(command, cancellationToken);
            }
        }
    }
}
