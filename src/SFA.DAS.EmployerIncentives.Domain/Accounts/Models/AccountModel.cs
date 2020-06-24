using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Models
{
    public class AccountModel : IEntityModel<long>
    {
        public long Id { get; set; }
        public ICollection<LegalEntityModel> LegalEntityModels { get; set; }
    }
}
