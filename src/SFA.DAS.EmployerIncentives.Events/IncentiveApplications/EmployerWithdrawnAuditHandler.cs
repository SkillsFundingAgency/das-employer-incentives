using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class EmployerWithdrawnAuditHandler : IDomainEventHandler<EmployerWithdrawn>
    {
        private readonly IIncentiveApplicationStatusAuditDataRepository _auditRepository;

        public EmployerWithdrawnAuditHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(EmployerWithdrawn @event, CancellationToken cancellationToken = default)
        {
            return _auditRepository.Add(new Domain.ValueObjects.IncentiveApplicationAudit(
                Guid.NewGuid(),
                @event.Model.Id,
                Enums.IncentiveApplicationStatus.EmployerWithdrawn,
                @event.ServiceRequest));
        }
    }
}
