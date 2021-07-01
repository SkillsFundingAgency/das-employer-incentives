using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class CompleteCommand : DomainCommand, ILockIdentifier, ILogWriter
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
        public Log Log
        {
            get
            {
                var message = $"Payment Process CompleteCommand for period {CollectionPeriod.PeriodNumber} and year {CollectionPeriod.AcademicYear} and date {CompletionDateTime}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
