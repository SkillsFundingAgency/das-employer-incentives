using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities
{
    public class GetLegalEntitiesResponse
    {
        public IEnumerable<LegalEntity> LegalEntities { get; set; }
    }
}