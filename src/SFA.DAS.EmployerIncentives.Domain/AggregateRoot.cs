using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain
{  
    public abstract class AggregateRoot<EntityId, Props> : Entity<EntityId, Props>, IAggregateRoot where Props : IEntityModel<EntityId>
    {
        protected AggregateRoot(EntityId id, Props properties, bool isNew) : base(id, properties, isNew) { }
    }
}
