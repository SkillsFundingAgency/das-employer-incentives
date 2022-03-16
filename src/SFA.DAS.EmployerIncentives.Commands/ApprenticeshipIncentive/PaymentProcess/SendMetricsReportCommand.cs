using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class SendMetricsReportCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
    {
        public Domain.ValueObjects.CollectionPeriod CollectionPeriod { get; }

        public string LockId { get => $"{nameof(Domain.ValueObjects.CollectionCalendar)}"; }

        public SendMetricsReportCommand(Domain.ValueObjects.CollectionPeriod collectionPeriod)
        {
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>("Generating metrics report", new object[] { }),
                    OnProcessed = () => new Tuple<string, object[]>("Metrics report generated and dispatchedfor collection period {collectionPeriod}", new object[] { CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>("Metrics report generation failed", new object[] { })
                };
            }
        }
    }
}
