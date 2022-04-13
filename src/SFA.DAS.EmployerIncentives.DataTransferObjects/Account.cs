using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects
{
    public class Account
    {
        public long AccountId { get; set; }
        public List<LegalEntity> LegalEntities { get; set; }
    }
}
