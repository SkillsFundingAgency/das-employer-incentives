using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class PaymentsResumedHandler : IDomainEventHandler<PaymentsResumed>
    {
        private readonly IIncentiveApplicationStatusAuditDataRepository _auditRepository;

        public PaymentsResumedHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(PaymentsResumed @event, CancellationToken cancellationToken = default)
        {
            return _auditRepository.Add(new Domain.ValueObjects.IncentiveApplicationAudit(
                Guid.NewGuid(),
                @event.Model.ApplicationApprenticeshipId,
                Enums.IncentiveApplicationStatus.PaymentsResumed,
                @event.ServiceRequest));
        }
    }
}
