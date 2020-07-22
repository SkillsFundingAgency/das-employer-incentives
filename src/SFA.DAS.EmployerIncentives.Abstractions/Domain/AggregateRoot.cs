namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public abstract class AggregateRoot<IdType, EntityModel> : Entity<IdType, EntityModel>, IAggregateRoot where EntityModel : IEntityModel<IdType>
    {
        protected AggregateRoot(IdType id, EntityModel properties, bool isNew) : base(id, properties, isNew) { }        
    }
}
