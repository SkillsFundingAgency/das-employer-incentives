using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod
{
    public class GetActiveCollectionPeriodResponse : IResponseLogWriter
    {
        public CollectionPeriodDto CollectionPeriod { get; }

        public GetActiveCollectionPeriodResponse(CollectionPeriodDto collectionPeriod)
        {
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public ResponseLog Log
        {
            get
            {
                return new ResponseLog
                {
                    OnProcessed = () => $"Active collection period number : {CollectionPeriod.CollectionPeriodNumber}, CollectionYear : {CollectionPeriod.CollectionYear}"
                };
            }
        }
    }
}
