using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities
{
    public class GetClawbackLegalEntitiesRequest : IQuery, IRequestLogWriterWithArgs
    {
        public bool IsSent { get; }
        public CollectionPeriod CollectionPeriod { get; }

        public GetClawbackLegalEntitiesRequest(CollectionPeriod collectionPeriod, bool isSent = false)
        {
            CollectionPeriod = collectionPeriod;
            IsSent = isSent;
        }

        [Newtonsoft.Json.JsonIgnore]
        public RequestLogWithArgs Log
        {
            get
            {
                var messagePrefix = IsSent ? "Getting sent" : " getting unsent";
                var message = messagePrefix + " clawbacks for collection period {collectionPeriod}";
                return new RequestLogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>(message, new object[] { CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>(message, new object[] { CollectionPeriod })
            };
            }
        }
    }
}
