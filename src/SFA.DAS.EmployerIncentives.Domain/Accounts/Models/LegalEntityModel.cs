using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Models
{
    public class LegalEntityModel : IEntityModel<long>
    {
        public long Id { get; set; }
        public string HashedLegalEntityId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string Name { get; set; }
        public int? SignedAgreementVersion { get; set; }
        public string VrfVendorId { get; set; }
        public string VrfCaseId { get; set; }
        public string VrfCaseStatus { get; set; }
        public DateTime? VrfCaseStatusLastUpdatedDateTime { get; set; }

        public BankDetailsStatus BankDetailsStatus { get; set; }
    }
}
