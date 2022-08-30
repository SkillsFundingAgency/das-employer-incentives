using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class RefreshEmploymentCheckCommand : DomainCommand, ILogWriter, IPeriodEndIncompatible
    {
        public string CheckType { get; }
        public long AccountLegalEntityId { get; }
        public long ULN { get; }
        public string ServiceRequestTaskId { get; }
        public string DecisionReference { get; }
        public DateTime ServiceRequestCreated { get; }
        
        public RefreshEmploymentCheckCommand(
            string checkType,
            long accountLegalEntityId,
            long uln,
            string serviceRequestTaskId,
            string decisionReference,
            DateTime? serviceRequestCreated)
        {
            CheckType = checkType;
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
