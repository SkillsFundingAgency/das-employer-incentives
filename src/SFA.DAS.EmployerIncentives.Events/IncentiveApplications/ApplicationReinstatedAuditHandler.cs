using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class ApplicationReinstatedAuditHandler : IDomainEventHandler<ApplicationReinstated>
    {
        private readonly IIncentiveApplicationStatusAuditDataRepository _auditRepository;

        public ApplicationReinstatedAuditHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(ApplicationReinstated @event, CancellationToken cancellationToken = default)
        {
            return _auditRepository.Add(new IncentiveApplicationAudit(
                Guid.NewGuid(),
                @event.Model.Id,
                IncentiveApplicationStatus.Reinstated,
                @event.ServiceRequest));
        }
    }
}
