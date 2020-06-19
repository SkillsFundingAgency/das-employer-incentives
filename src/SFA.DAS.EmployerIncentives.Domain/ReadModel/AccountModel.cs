using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.Data
{
    public class AccountModel : IAccountModel
    {
        public long Id { get; set; }
        public ICollection<ILegalEntityModel> LegalEntityModels { get; set; }
    }
}
