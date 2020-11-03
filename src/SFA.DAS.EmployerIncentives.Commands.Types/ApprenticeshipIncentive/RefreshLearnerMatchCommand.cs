using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class RefreshLearnerCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; private set; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public RefreshLearnerCommand(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive RefreshLearnerCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
