using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmployerIncentives.Data.Tables
{
    [Table("Accounts")]
    public class Account
    {
        [ExplicitKey]
        public long Id { get; set; }
        [ExplicitKey]
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
    }
}
