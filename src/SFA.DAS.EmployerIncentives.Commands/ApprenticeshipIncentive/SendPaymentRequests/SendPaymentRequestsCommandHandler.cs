using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
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
        private readonly ILogger<SendPaymentRequestsCommandHandler> _logger;

        public SendPaymentRequestsCommandHandler(
            IAccountDataRepository accountRepository,
            IPayableLegalEntityQueryRepository queryRepository,
            IBusinessCentralFinancePaymentsService businessCentralFinancePaymentsService,
            ILogger<SendPaymentRequestsCommandHandler> logger)
        {
            _accountRepository = accountRepository;
            _queryRepository = queryRepository;
            _businessCentralFinancePaymentsService = businessCentralFinancePaymentsService;
            _logger = logger;
        }

        public async Task Handle(SendPaymentRequestsCommand command, CancellationToken cancellationToken = default)
        {
            IList<PaymentDto> payments;
            IList<PaymentDto> paymentsToSend;
            do
            {
                payments = await _queryRepository.GetPaymentsToSendForAccountLegalEntity(command.AccountLegalEntityId);
                _logger.LogInformation("[SendPaymentRequestsCommandHandler] Number of outstanding payments for {AccountLegalEntityId} found is: {Payments}",
                    command.AccountLegalEntityId, payments.Count);

                if (!payments.Any()) return;

                paymentsToSend = payments.Take(_businessCentralFinancePaymentsService.PaymentRequestsLimit).ToList();

                await _businessCentralFinancePaymentsService.SendPaymentRequests(paymentsToSend);

                await _accountRepository.UpdatePaidDateForPaymentIds(paymentsToSend.Select(s => s.PaymentId).ToList(),
                    command.AccountLegalEntityId, command.PaidDate);

            } while (payments.Count > paymentsToSend.Count);
        }
    }
}
