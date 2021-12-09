using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class UpdateEmploymentCheckCommand : DomainCommand, ILockIdentifier, ILogWriter, IPeriodEndIncompatible
    {
        public Guid CorrelationId { get; }
        public EmploymentCheckResultType Result { get; }
        public DateTime DateChecked { get; }
        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.EmploymentCheck)}_{CorrelationId}"; }
        public TimeSpan CommandDelay => TimeSpan.FromMinutes(15);

        public bool CancelCommand => false;

        public UpdateEmploymentCheckCommand(Guid correlationId, EmploymentCheckResultType result, DateTime dateChecked)
        {
            CorrelationId = correlationId;
            Result = result;
            DateChecked = dateChecked;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"UpdateEmploymentCheckCommand for CorrelationId {CorrelationId}, Result {Result} and DateChecked {DateChecked}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
