using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain
{
    public abstract class AggregateRoot<IdType, EntityModel> : Entity<IdType, EntityModel>, IAggregateRoot where EntityModel : IEntityModel<IdType>
    {
        protected AggregateRoot(IdType id, EntityModel properties, bool isNew) : base(id, properties, isNew) { }

        #region for persisting objects
        public EntityModel GetModel()
        {
            return Model;
        }
        #endregion
    }
}
