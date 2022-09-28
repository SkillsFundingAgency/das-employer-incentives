using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class SendEmploymentCheckRequestsCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; private set; }
        public EmploymentCheckType CheckType { get; private set; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public SendEmploymentCheckRequestsCommand(Guid apprenticeshipIncentiveId, EmploymentCheckType checkType)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            CheckType = checkType;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive SendEmploymentCheckRequestsCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and Check Type {CheckType}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
