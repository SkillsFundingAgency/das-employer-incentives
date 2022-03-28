using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects
{
    public class AccountDto
    {
        public long AccountId { get; set; }
        public List<LegalEntityDto> LegalEntities { get; set; }
    }
}
