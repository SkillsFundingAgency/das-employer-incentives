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
        public string LegalEntityName { get; set; }
        public bool HasSignedIncentivesTerms { get; set; }
    }
}
