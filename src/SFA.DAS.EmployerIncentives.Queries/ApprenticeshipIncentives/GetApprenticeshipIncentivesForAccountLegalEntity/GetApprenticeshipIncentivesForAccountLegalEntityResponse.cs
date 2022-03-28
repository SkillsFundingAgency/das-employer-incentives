using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity
{
    public class GetApprenticeshipIncentivesForAccountLegalEntityResponse
    {
        public List<ApprenticeshipIncentive> ApprenticeshipIncentives { get; }

        public GetApprenticeshipIncentivesForAccountLegalEntityResponse(List<ApprenticeshipIncentive> apprenticeshipIncentives)
        {
            ApprenticeshipIncentives = apprenticeshipIncentives;
        }
    }
}
