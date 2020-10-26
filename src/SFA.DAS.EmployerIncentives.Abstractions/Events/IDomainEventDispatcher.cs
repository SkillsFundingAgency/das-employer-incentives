using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Events
{
    public interface IDomainEventDispatcher
    {
        Task Send<TDomainEvent>(TDomainEvent @event, CancellationToken cancellationToken = default(CancellationToken)) where TDomainEvent : IDomainEvent;
    }
}
