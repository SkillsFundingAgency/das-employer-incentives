using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class PaymentRevertedAuditHandler : IDomainEventHandler<PaymentReverted>
    {
        public Task Handle(PaymentReverted @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
