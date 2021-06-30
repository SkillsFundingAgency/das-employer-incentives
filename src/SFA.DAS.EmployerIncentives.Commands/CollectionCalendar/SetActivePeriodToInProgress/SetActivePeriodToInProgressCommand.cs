using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionCalendar.SetActivePeriodToInProgress
{
    public class SetActivePeriodToInProgressCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public string LockId { get => $"{nameof(SetActivePeriodToInProgressCommand)}"; }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"SetActivePeriodToInProgressCommand";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
