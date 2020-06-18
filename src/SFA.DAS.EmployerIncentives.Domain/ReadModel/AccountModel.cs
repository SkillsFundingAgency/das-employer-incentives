using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain.Data
{
    public class AccountModel : IAccountModel
    {
        public long Id { get; set; }
        public long AccountLegalEntityId { get; set; }
        public ILegalEntityModel LegalEntityModel { get; set; }
    }
}
