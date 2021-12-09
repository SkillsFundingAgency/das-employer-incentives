using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks
{
    public class SendClawbacksCommand : DomainCommand, ILogWriterWithArgs
    {
        public long AccountLegalEntityId { get; }
        public DateTime ClawbackDate { get; }
        public Domain.ValueObjects.CollectionPeriod CollectionPeriod { get; }

        public SendClawbacksCommand(long accountLegalEntityId, DateTime clawbackDate, Domain.ValueObjects.CollectionPeriod collectionPeriod)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ClawbackDate = clawbackDate;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                var message = "[SendClawbacksForAccountLegalEntity] Publish SendClawbackRequestsCommand for account legal entity { accountLegalEntityId}, collection period {collectionPeriod}";

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
