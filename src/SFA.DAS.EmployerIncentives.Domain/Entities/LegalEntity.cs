using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain.Entities
{
    public class LegalEntity : Entity<long, ILegalEntityModel>
    {
        public string Name => Model.Name;

        public static LegalEntity New(long id, string name)
        {
            
            var model = new LegalEntityModel
            {
                Name = name
            };
            
            return new LegalEntity(id, model, true);
        }

        public static LegalEntity Create(ILegalEntityModel model)
        {
            return new LegalEntity(model.Id, model);
        }

        private LegalEntity(long id, ILegalEntityModel model, bool isNew = false) : base(id, model, isNew)
        {
        }
    }
}
