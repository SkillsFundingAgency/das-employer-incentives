using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts
{
    public sealed class LegalEntity : Entity<long, LegalEntityModel>
    {
        public string Name => Model.Name;
        public bool HasSignedAgreementTerms => Model.HasSignedAgreementTerms;

        public static LegalEntity New(long id, string name)
        {            
            var model = new LegalEntityModel
            {
                Name = name,
                HasSignedAgreementTerms = false
            };
            
            return new LegalEntity(id, model, true);
        }

        public static LegalEntity Create(LegalEntityModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id == default) throw new ArgumentException("Id is not set", nameof(model));
            return new LegalEntity(model.Id, model);
        }

        private LegalEntity(long id, LegalEntityModel model, bool isNew = false) : base(id, model, isNew)
        {
        }

        public void SignedAgreement(int signedAgreementVersion, int minimumRequiredVersion)
        {
            if (signedAgreementVersion >= minimumRequiredVersion)
            {
                Model.HasSignedAgreementTerms = true;
            }
        }
    }
}
