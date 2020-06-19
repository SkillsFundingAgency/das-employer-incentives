using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.Map;
using System.Linq;
using System.Collections.ObjectModel;

namespace SFA.DAS.EmployerIncentives.Domain.Entities
{
    public class Account : AggregateRoot<long, IAccountModel>
    {   
        public IReadOnlyCollection<LegalEntity> LegalEntities => Model.LegalEntityModels.Map().ToList().AsReadOnly();
     
        public static Account New(long id)
        {
            return new Account(id, new AccountModel() { LegalEntityModels = new Collection<ILegalEntityModel>() } , true);
        }

        public static Account Create(IAccountModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id == default) throw new ArgumentException("Id is not set", nameof(model));
            return new Account(model.Id, model);
        }

        public void AddLegalEntity(long accountLegalEntityId, LegalEntity legalEntity)
        {   
            if (Model.LegalEntityModels.Any(i => i.AccountLegalEntityId.Equals(accountLegalEntityId)))
            {
                throw new InvalidMethodCallException("Legal entity has already been set up");
            }

            Model.LegalEntityModels.Add(new LegalEntityModel { Id = legalEntity.Id, Name = legalEntity.Name, AccountLegalEntityId = accountLegalEntityId });
        }

        private Account(long id, IAccountModel model, bool isNew = false) : base(id, model, isNew)
        {            
        }
    }
}
