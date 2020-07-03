namespace SFA.DAS.EmployerIncentives.Data.Models
{
    public partial class Accounts
    {
        public long Id { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
    }
}
