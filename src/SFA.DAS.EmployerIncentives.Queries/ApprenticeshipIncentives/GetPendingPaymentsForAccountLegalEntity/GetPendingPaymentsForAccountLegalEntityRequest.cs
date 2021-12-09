using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityRequest : IQuery, IRequestLogWriterWithArgs
    {
        public long AccountLegalEntityId { get; }

        public CollectionPeriod CollectionPeriod { get; }

        public GetPendingPaymentsForAccountLegalEntityRequest(long accountLegalEntityId, CollectionPeriod collectionPeriod)
        {
            AccountLegalEntityId = accountLegalEntityId;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public RequestLogWithArgs Log
        {
            get
            {
                var message = "Getting pending payments for account legal entity {AccountLegalEntityId}, collection period {CollectionPeriod}";
                return new RequestLogWithArgs
                {
                    OnProcessing = () => new System.Tuple<string, object[]>(message, new object[] { AccountLegalEntityId, CollectionPeriod }),
                    OnError = () => new System.Tuple<string, object[]>(message, new object[] { AccountLegalEntityId, CollectionPeriod })
                };
            }
        }
    }
}
