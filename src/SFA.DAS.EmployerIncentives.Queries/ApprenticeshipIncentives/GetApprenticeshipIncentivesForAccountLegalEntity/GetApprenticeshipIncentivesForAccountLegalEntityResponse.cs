using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity
{
    public class GetApprenticeshipIncentivesForAccountLegalEntityResponse
    {
        public List<ApprenticeshipIncentiveDto> ApprenticeshipIncentives { get; }

        public GetApprenticeshipIncentivesForAccountLegalEntityResponse(List<ApprenticeshipIncentiveDto> apprenticeshipIncentives)
        {
            ApprenticeshipIncentives = apprenticeshipIncentives;
        }
    }
}
