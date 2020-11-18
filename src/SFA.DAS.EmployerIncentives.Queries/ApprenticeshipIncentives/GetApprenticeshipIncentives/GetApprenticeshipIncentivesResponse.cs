using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives
{
    public class GetApprenticeshipIncentivesResponse
    {
        public List<ApprenticeshipIncentiveDto> ApprenticeshipIncentives { get; }

        public GetApprenticeshipIncentivesResponse(List<ApprenticeshipIncentiveDto> apprenticeshipIncentives)
        {
            ApprenticeshipIncentives = apprenticeshipIncentives;
        }
    }
}
