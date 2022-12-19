using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class EmploymentChecksCreatedAuditHandler : IDomainEventHandler<EmploymentChecksCreated>
    {
        private readonly IEmploymentCheckAuditRepository _auditRepository;

        public EmploymentChecksCreatedAuditHandler(
            IEmploymentCheckAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(EmploymentChecksCreated @event, CancellationToken cancellationToken = default)
        {
            if(@event.ServiceRequest == null)
            {
                return Task.CompletedTask;
            }

            return _auditRepository.Add(new EmploymentCheckRequestAudit(                
                Guid.NewGuid(),
                @event.ApprenticeshipIncentiveId,
                @event.Model.CheckType,
                @event.ServiceRequest));
        }
    }
}
