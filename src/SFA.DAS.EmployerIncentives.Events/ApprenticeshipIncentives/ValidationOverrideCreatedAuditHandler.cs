using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class ValidationOverrideCreatedAuditHandler : IDomainEventHandler<ValidationOverrideCreated>
    {
        private readonly IValidationOverrideAuditRepository _auditRepository;

        public ValidationOverrideCreatedAuditHandler(
            IValidationOverrideAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(ValidationOverrideCreated @event, CancellationToken cancellationToken = default)
        {            
            return _auditRepository.Add(new ValidationOverrideStepAudit(
                @event.ValidationOverrideId,
                @event.ApprenticeshipIncentiveId,
                @event.ValidationOverrideStep,
                @event.ServiceRequest));
        }
    }
}
