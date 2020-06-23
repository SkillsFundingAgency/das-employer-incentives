using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Entities
{
    public abstract class Entity<IdType, EntityModel>  where EntityModel : IEntityModel<IdType>
    {
        public IdType Id => Model.Id;
        protected EntityModel Model { get; set; }
        public bool IsNew { get; protected set; }

        protected Entity(IdType id, EntityModel model, bool isNew)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Model = model;
            Model.Id = id;
            IsNew = isNew;
        }       

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }
            if(this == obj)
            {
                return true;
            }
            if(GetType() != obj.GetType())
            {
                return false;
            }           
            
            return Id.Equals(obj.GetType().GetProperty("Id").GetValue(obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (Id is object ? Id.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
