using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class CompleteCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
    {
        public DateTime CompletionDateTime { get; }
        public Domain.ValueObjects.CollectionPeriod CollectionPeriod { get; }

        public string LockId { get => $"{nameof(Domain.ValueObjects.CollectionCalendar)}"; }

        public CompleteCommand(DateTime completionDateTime, Domain.ValueObjects.CollectionPeriod collectionPeriod)
        {
            CompletionDateTime = completionDateTime;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>("Completing payment process", new object[] { }),
                    OnProcessed = () => new Tuple<string, object[]>("Payment process completed for collection period {collectionPeriod}", new object[] { CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>("Completing payment process", new object[] { })
                };
            }
        }
    }
}
