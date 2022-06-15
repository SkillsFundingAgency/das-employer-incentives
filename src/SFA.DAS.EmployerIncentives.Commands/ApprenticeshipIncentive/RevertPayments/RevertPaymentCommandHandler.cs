using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments
{
    public class RevertPaymentCommandHandler : ICommandHandler<RevertPaymentCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public RevertPaymentCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }

        public async Task Handle(RevertPaymentCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.FindByPaymentId(command.PaymentId);

            incentive.RevertPayment(command.PaymentId, new ServiceRequest(command.ServiceRequestId, command.DecisionReferenceNumber, command.DateServiceRequestTaskCreated.Value));

            await _incentiveDomainRepository.Save(incentive);
        }
    }
}
