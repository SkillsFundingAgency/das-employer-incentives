using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesResponse
    {
        public List<PayableLegalEntityDto> PayableLegalEntities { get; }

        public GetPayableLegalEntitiesResponse(List<PayableLegalEntityDto> legalEntities)
        {
            PayableLegalEntities = legalEntities;
        }
    }
}
