using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs
{
    public class AccountDto
    {
        public long AccountId { get; set; }
        public List<LegalEntityDto> LegalEntities { get; set; }
    }
}
