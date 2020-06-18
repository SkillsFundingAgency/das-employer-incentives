using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain.Data
{
    public class LegalEntityModel : ILegalEntityModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
