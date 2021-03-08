using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LegalEntity : ValueObject
    {
        public Account  Account { get; }
        public long LegalEntityId { get; }
        public string LegalEntityName { get; set; }
        public string VrfVendorId { get; set; }
        public string VrfCaseStatus { get; set; }
        public string HashedLegalEntityId { get; set; }

        public LegalEntity(Account account, long legalEntityId, string legalEntityName, string vrfVendorId, 
                           string vrfCaseStatus, string hashedLegalEntityId)
        {
            Account = account;
            LegalEntityId = legalEntityId;
            LegalEntityName = legalEntityName;
            VrfVendorId = vrfVendorId;
            VrfCaseStatus = vrfCaseStatus;
            HashedLegalEntityId = hashedLegalEntityId;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Account;
            yield return LegalEntityId;
            yield return LegalEntityName;
            yield return VrfVendorId;
            yield return HashedLegalEntityId;
        }
    }
}
