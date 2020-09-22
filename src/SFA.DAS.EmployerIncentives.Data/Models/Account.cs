using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    [Table("Accounts")]
    public partial class Account
    {
        public long Id { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string HashedLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public bool HasSignedIncentivesTerms { get; set; }
        public string VrfVendorId { get; set; }
        public string VrfCaseId { get; set; }
        public string VrfCaseStatus { get; set; }
        public DateTime? VrfCaseStatusLastUpdatedDateTime { get; set; }
    }
}
