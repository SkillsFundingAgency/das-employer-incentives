﻿using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod
{
    public class GetActiveCollectionPeriodResponse : IResponseLogWriter
    {
        public CollectionPeriod CollectionPeriod { get; }

        public GetActiveCollectionPeriodResponse(CollectionPeriod collectionPeriod)
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
