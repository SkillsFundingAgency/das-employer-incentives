using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class PendingPaymentReinstatedAuditHandler : IDomainEventHandler<PendingPaymentReinstated>
    {
        private readonly IReinstatedPendingPaymentAuditRepository _auditRepository;

        public PendingPaymentReinstatedAuditHandler(IReinstatedPendingPaymentAuditRepository auditRepository)
        {
            _auditRepository = auditRepository; 
        }

        public async Task Handle(PendingPaymentReinstated @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var auditRecord = new ReinstatedPendingPaymentAudit(
                Guid.NewGuid(),
                @event.Model.ApprenticeshipIncentiveId,
                @event.Model.Id,
                new ReinstatePaymentRequest(
                    @event.ReinstatePaymentRequest.TaskId,
                    @event.ReinstatePaymentRequest.DecisionReference,
                    @event.ReinstatePaymentRequest.Created,
                    @event.ReinstatePaymentRequest.Process),
                DateTime.UtcNow);

            await _auditRepository.Add(auditRecord);
        }
    }
}
