using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Models
{
    public class LegalEntityModel : IEntityModel<long>
    {
        public long Id { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string Name { get; set; }
        public bool HasSignedAgreementTerms { get; set; }
    }
}
