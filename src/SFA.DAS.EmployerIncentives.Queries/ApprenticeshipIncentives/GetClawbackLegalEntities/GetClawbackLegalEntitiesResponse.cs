using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities
{
    public class GetClawbackLegalEntitiesResponse
    {
        public List<ClawbackLegalEntityDto> ClawbackLegalEntities { get; }

        public GetClawbackLegalEntitiesResponse(List<ClawbackLegalEntityDto> legalEntities)
        {
            ClawbackLegalEntities = legalEntities;
        }
    }
}
