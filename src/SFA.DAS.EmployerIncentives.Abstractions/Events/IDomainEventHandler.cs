using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Events
{
    public interface IDomainEventHandler<in T> where T: IDomainEvent
    {
        Task Handle(T @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
