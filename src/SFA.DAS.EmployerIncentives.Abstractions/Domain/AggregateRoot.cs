using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public abstract class AggregateRoot<IdType, EntityModel> : Entity<IdType, EntityModel>, IAggregateRoot where EntityModel : IEntityModel<IdType>
    {
        private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

        protected AggregateRoot(IdType id, EntityModel properties, bool isNew) : base(id, properties, isNew) { }

        protected void AddEvent(IDomainEvent @event)
        {
            lock (_events)
            {
                _events.Add(@event);
            }
        }

        protected bool Contains<T>() where T : IDomainEvent
        {
            foreach(var domainEvent in _events)
            {
                if(domainEvent is T)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<IDomainEvent> FlushEvents()
        {
            lock (_events)
            {
                var events = _events.ToArray();
                _events.Clear();
                return events;
            }
        }
    }
}
