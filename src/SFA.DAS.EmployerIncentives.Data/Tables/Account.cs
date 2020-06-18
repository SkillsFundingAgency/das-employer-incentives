using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmployerIncentives.Data.Tables
{
    [Table("dbo.Accounts")]
    public class Account
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
    }
}
