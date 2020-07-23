using SFA.DAS.EmployerIncentives.Abstractions;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

namespace SFA.DAS.EmployerIncentives.Queries.Account
{
    public class GetLegalEntitiesResponse
    {
        public IEnumerable<LegalEntityDto> LegalEntities { get; set; }
    }
}