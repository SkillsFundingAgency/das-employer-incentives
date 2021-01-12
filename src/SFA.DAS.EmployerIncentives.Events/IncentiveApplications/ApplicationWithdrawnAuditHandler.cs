using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public abstract class ApplicationWithdrawnAuditHandler<T> : IDomainEventHandler<T> where T : ApplicationWithdrawn
    {
        private readonly IIncentiveApplicationStatusAuditDataRepository _auditRepository;

        protected ApplicationWithdrawnAuditHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(T @event, CancellationToken cancellationToken = default)
        {
            return _auditRepository.Add(new Domain.ValueObjects.IncentiveApplicationAudit(
                Guid.NewGuid(),
                @event.Model.Id,
                @event.WithdrawalStatus,
                @event.ServiceRequest));
        }
    }
}
