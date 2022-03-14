using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class ValidationOverrideDeletedAuditHandler : IDomainEventHandler<ValidationOverrideDeleted>
    {
        private readonly IValidationOverrideAuditRepository _auditRepository;

        public ValidationOverrideDeletedAuditHandler(
            IValidationOverrideAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(ValidationOverrideDeleted @event, CancellationToken cancellationToken = default)
        {
            return _auditRepository.Delete(@event.ValidationOverrideId);
        }
    }
}
