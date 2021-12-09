using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesRequest : IQuery, IRequestLogWriterWithArgs
    {
        public CollectionPeriod CollectionPeriod { get; }

        public GetPayableLegalEntitiesRequest(CollectionPeriod collectionPeriod)
        {
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public RequestLogWithArgs Log
        {
            get
            {
                var message = "Getting payable legal entities for collection period {collectionPeriod}.";
                return new RequestLogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>(message, new object[] { CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>(message, new object[] { CollectionPeriod })
                };
            }
        }
    }
}
