using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests
{
    public class SendPaymentRequestsCommandHandler : ICommandHandler<SendPaymentRequestsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IPayableLegalEntityQueryRepository _queryRepository;
        private readonly IBusinessCentralFinancePaymentsService _businessCentralFinancePaymentsService;

        public SendPaymentRequestsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository,
            IPayableLegalEntityQueryRepository queryRepository,
            IBusinessCentralFinancePaymentsService businessCentralFinancePaymentsService)
        {
            _domainRepository = domainRepository;
            _queryRepository = queryRepository;
            _businessCentralFinancePaymentsService = businessCentralFinancePaymentsService;
        }

        public async Task Handle(SendPaymentRequestsCommand command, CancellationToken cancellationToken = default)
        {
            var payments = await _queryRepository.GetPaymentsToSendForAccountLegalEntity(command.AccountLegalEntity);

            var sent = await _businessCentralFinancePaymentsService.SendPaymentRequestsForLegalEntity(payments);

            // update payments PaidDate
            // Question is do we do this for each ApprenticeshipIncentive or via the AccountLegalEntity record??

            // if not all payments sent sent command again
            if (!sent.AllPaymentsSent)
            {
                // Send new NSB Message to send the next lot of payment
            }
        }
    }
}
