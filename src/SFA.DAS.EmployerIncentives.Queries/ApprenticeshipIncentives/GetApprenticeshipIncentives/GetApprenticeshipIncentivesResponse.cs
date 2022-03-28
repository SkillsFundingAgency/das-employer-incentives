using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives
{
    public class GetApprenticeshipIncentivesResponse: IResponseLogWriter
    {
        public List<ApprenticeshipIncentive> ApprenticeshipIncentives { get; }

        public GetApprenticeshipIncentivesResponse(List<ApprenticeshipIncentive> apprenticeshipIncentives)
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
