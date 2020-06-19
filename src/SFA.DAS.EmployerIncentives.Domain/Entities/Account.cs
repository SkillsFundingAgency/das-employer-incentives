using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Entities
{
    public class Account : AggregateRoot<long, IAccountModel>
    {
        public LegalEntity LegalEntity => LegalEntity.Create(Model.LegalEntityModel);
        public long AccountLegalEntityId => Model.AccountLegalEntityId;

        public static Account New(long id)
        {
            return new Account(id, new AccountModel(), true);
        }

        public static Account Create(IAccountModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id == default) throw new ArgumentException("Id is not set", nameof(model));
            return new Account(model.Id, model);
        }

        public void AddLegalEntity(long accountLegalEntityId, LegalEntity legalEntity)
        {
            if (Model.LegalEntityModel != null)
            {
                throw new InvalidMethodCallException("Legal entity has already been set up");
            }

            Model.AccountLegalEntityId = accountLegalEntityId;
            Model.LegalEntityModel = legalEntity.GetModel();
        }

        private Account(long id, IAccountModel model, bool isNew = false) : base(id, model, isNew)
        {
        }
    }
}
