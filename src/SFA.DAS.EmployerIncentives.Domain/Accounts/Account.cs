using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Map;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts
{
    public sealed class Account : AggregateRoot<long, AccountModel>
    {   
        public IReadOnlyCollection<LegalEntity> LegalEntities => Model.LegalEntityModels.Map().ToList().AsReadOnly();
     
        public static Account New(long id)
        {
            return new Account(id, new AccountModel() { LegalEntityModels = new Collection<LegalEntityModel>() } , true);
        }

        public static Account Create(AccountModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id == default) throw new ArgumentException("Id is not set", nameof(model));
            return new Account(model.Id, model);
        }

        public LegalEntity GetLegalEntity(long accountLegalEntityId)
        {
            var legalEntityModel = Model.LegalEntityModels.SingleOrDefault(l => l.AccountLegalEntityId == accountLegalEntityId);
            return legalEntityModel == null ? null : LegalEntity.Create(legalEntityModel);
        }

        public void RemoveLegalEntity(LegalEntity legalEntity)
        {
            var accountLegalEntityId = legalEntity.GetModel().AccountLegalEntityId;
            var legalEntityModel = Model.LegalEntityModels.SingleOrDefault(l => l.AccountLegalEntityId == accountLegalEntityId);
            if (legalEntityModel != null)
            {
                Model.LegalEntityModels.Remove(legalEntityModel);
            }
        }

        public void AddLegalEntity(long accountLegalEntityId, LegalEntity legalEntity)
        {   
            if (Model.LegalEntityModels.Any(i => i.AccountLegalEntityId.Equals(accountLegalEntityId)))
            {
                throw new LegalEntityAlreadyExistsException("Legal entity has already been added");
            }

            var legalEntityModel = legalEntity.GetModel();
            legalEntityModel.AccountLegalEntityId = accountLegalEntityId;
            Model.LegalEntityModels.Add(legalEntityModel);
        }

        private Account(long id, AccountModel model, bool isNew = false) : base(id, model, isNew)
        {            
        }
    }
}
