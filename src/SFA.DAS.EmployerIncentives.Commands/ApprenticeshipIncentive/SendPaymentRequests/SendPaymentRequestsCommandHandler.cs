using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

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

            var sent = await _businessCentralFinancePaymentsService.SendPaymentRequestsForLegalEntity(payments);

            await _accountRepository.UpdatePaidDateForPaymentIds(sent.PaymentsSent.Select(s => s.PaymentId).ToList(), command.AccountLegalEntityId, command.PaidDate);

            // if not all payments sent sent command again
            if (!sent.AllPaymentsSent)
            {
                // Send new NSB Message to send the next lot of payment
            }
        }
    }
}
