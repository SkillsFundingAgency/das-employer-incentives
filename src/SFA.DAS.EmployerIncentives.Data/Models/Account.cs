using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    [Table("Accounts")]
    public partial class Account
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public long Id { get; set; }

        public long AccountLegalEntityId { get; set; }

        public long LegalEntityId { get; set; }

        [StringLength(6)]
        public string HashedLegalEntityId { get; set; }

        public string LegalEntityName { get; set; }

        public int? SignedAgreementVersion { get; set; }

        public string VrfVendorId { get; set; }

        public string VrfCaseId { get; set; }

        public string VrfCaseStatus { get; set; }

        public DateTime? VrfCaseStatusLastUpdatedDateTime { get; set; }
        public DateTime? VendorBlockEndDate { get; set; }
    }
}
