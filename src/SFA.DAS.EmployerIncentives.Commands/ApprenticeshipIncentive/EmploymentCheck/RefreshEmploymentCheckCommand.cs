using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Commands.Types;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentCheckCommand : DomainCommand, ILockIdentifier, ILogWriter, IPeriodEndIncompatible
    {
        public long AccountLegalEntityId { get; }
        public long ULN { get; }
        public string ServiceRequestTaskId { get; }
        public string DecisionReference { get; }
        public DateTime ServiceRequestCreated { get; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ULN}"; }

        public RefreshEmploymentCheckCommand(
            long accountLegalEntityId,
            long uln,
            string serviceRequestTaskId,
            string decisionReference,
            DateTime? serviceRequestCreated)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
            ServiceRequestTaskId = serviceRequestTaskId;
            DecisionReference = decisionReference;
            ServiceRequestCreated = serviceRequestCreated ?? DateTime.Now;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive RefreshEmploymentCheckCommand for ServiceRequestTaskId {ServiceRequestTaskId}, AccountLegalEntityId {AccountLegalEntityId}, ULN {ULN}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }

        public TimeSpan CommandDelay => TimeSpan.FromMinutes(15);
        public bool CancelCommand => false;
    }
}
