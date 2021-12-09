using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives
{
    public class GetApprenticeshipIncentivesRequest : IQuery, IRequestLogWriter
    {
        [Newtonsoft.Json.JsonIgnore]
        public RequestLog Log
        {
            get
            {
                var message = "Getting all Apprenticeship Incentives";
                return new RequestLog
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
