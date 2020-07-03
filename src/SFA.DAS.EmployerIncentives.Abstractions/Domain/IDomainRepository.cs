using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public interface IDomainRepository<EntityId, T> where T: IAggregateRoot
    {
        Task<T> Find(EntityId id);
        Task Save(T aggregate);
    }
}
