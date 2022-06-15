using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class PaymentRevertedAuditHandler : IDomainEventHandler<PaymentReverted>
    {
        private readonly IRevertedPaymentAuditRepository _revertedPaymentAuditRepository;

        public PaymentRevertedAuditHandler(IRevertedPaymentAuditRepository revertedPaymentAuditRepository)
        {
            _revertedPaymentAuditRepository = revertedPaymentAuditRepository;
        }

        public async Task Handle(PaymentReverted @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var revertedPaymentAudit = new RevertedPaymentAudit(
                Guid.NewGuid(),
                @event.Model.ApprenticeshipIncentiveId,
                @event.Model.Id,
                @event.Model.PendingPaymentId,
                @event.Model.PaymentPeriod,
                @event.Model.PaymentYear,
                @event.Model.Amount,
                @event.Model.CalculatedDate,
                @event.Model.PaidDate.Value,
                @event.Model.VrfVendorId,
                @event.ServiceRequest,
                DateTime.UtcNow
            );

            await _revertedPaymentAuditRepository.Add(revertedPaymentAudit);
        }
    }
}
