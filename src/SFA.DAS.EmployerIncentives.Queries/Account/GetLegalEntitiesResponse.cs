using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Queries.Account
{
    public class GetLegalEntitiesResponse
    {
        public IEnumerable<LegalEntityDto> LegalEntities { get; set; }
    }

    public class LegalEntityDto
    {
    }
}