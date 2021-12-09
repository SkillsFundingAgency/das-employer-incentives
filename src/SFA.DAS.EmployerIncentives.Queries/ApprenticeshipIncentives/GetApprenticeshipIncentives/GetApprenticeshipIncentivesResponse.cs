using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives
{
    public class GetApprenticeshipIncentivesResponse: IResponseLogWriter
    {
        public List<ApprenticeshipIncentiveDto> ApprenticeshipIncentives { get; }

        public GetApprenticeshipIncentivesResponse(List<ApprenticeshipIncentiveDto> apprenticeshipIncentives)
        {
            ApprenticeshipIncentives = apprenticeshipIncentives;
        }

        [Newtonsoft.Json.JsonIgnore]
        public ResponseLog Log
        {
            get
            {
                return new ResponseLog
                {
                    OnProcessed = () => $"{ApprenticeshipIncentives.Count} returned"
                };
            }
        }
    }
}
