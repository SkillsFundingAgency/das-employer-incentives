using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Events.VendorBlocks
{
    public class VendorBlockCreatedAuditHandler : IDomainEventHandler<VendorBlockCreated>
    {
        private readonly IVendorBlockAuditRepository _auditRepository;

        public VendorBlockCreatedAuditHandler(IVendorBlockAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public Task Handle(VendorBlockCreated @event, CancellationToken cancellationToken = default)
        {
            if (@event.ServiceRequest == null)
            {
                return Task.CompletedTask;
            }

            return _auditRepository.Add(new VendorBlockRequestAudit(Guid.NewGuid(), @event.VendorId, @event.VendorBlockEndDate, @event.ServiceRequest));
        }
    }
}
