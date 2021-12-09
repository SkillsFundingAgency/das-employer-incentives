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
                var message = "active collection period to in progress";
                return new Log
                {
                    OnProcessing = () => "Setting " +  message,
                    OnProcessed = () => "Set " + message,
                    OnError = () => message
                };
            }
        }
    }
}
