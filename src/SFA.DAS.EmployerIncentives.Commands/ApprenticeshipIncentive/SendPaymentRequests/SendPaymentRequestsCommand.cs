using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests
{
    public class SendPaymentRequestsCommand : DomainCommand, ILogWriterWithArgs
    {
        public long AccountLegalEntityId { get; }
        public DateTime PaidDate { get; }

        public Domain.ValueObjects.CollectionPeriod CollectionPeriod { get; }

        public SendPaymentRequestsCommand(long accountLegalEntityId, DateTime paidDate, Domain.ValueObjects.CollectionPeriod collectionPeriod)
        {
            AccountLegalEntityId = accountLegalEntityId;
            PaidDate = paidDate;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                var message = "[SendPaymentRequestsForAccountLegalEntity] Publish SendPaymentRequestsCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}";

                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>(message, new object[] { AccountLegalEntityId, CollectionPeriod }),
                    OnProcessed = () => new Tuple<string, object[]>(message, new object[] { AccountLegalEntityId, CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>(message, new object[] { AccountLegalEntityId, CollectionPeriod })
                };
            }
        }
    }
}
