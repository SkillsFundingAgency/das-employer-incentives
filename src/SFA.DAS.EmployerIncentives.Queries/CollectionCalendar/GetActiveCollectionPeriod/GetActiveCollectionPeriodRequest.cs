using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod
{
    public class GetActiveCollectionPeriodRequest : IQuery, IRequestLogWriter
    {
        [Newtonsoft.Json.JsonIgnore]
        public RequestLog Log
        {
            get
            {
                var message = "Getting active collection period";
                return new RequestLog
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
