using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Models
{
    public class LegalEntityModel : IEntityModel<long>
    {
        public long Id { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string Name { get; set; }
    }
}
