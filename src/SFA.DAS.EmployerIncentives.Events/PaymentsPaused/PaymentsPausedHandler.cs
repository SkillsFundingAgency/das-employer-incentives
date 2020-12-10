using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;

namespace SFA.DAS.EmployerIncentives.Events.PaymentsPaused
{
    public class PaymentsPausedHandler : IDomainEventHandler<Domain.ApprenticeshipIncentives.Events.PaymentsPaused>
    {
        private readonly IIncentiveApplicationStatusAuditDataRepository _auditRepository;

        public PaymentsPausedHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(Domain.ApprenticeshipIncentives.Events.PaymentsPaused @event, CancellationToken cancellationToken = default)
        {
            return _auditRepository.Add(new Domain.ValueObjects.IncentiveApplicationAudit(
                Guid.NewGuid(),
                @event.Model.ApplicationApprenticeshipId,
                Enums.IncentiveApplicationStatus.PaymentsPaused,
                @event.ServiceRequest));
        }
    }
}
