using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Persistence
{
    public interface IDomainRepository<EntityId, T> where T: IAggregateRoot
    {
        Task<T> Find(EntityId id);
        Task Save(T aggregate);
    }
}
