using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class ValidationOverrideCommand : DomainCommand, ILogWriter, IPeriodEndIncompatible
    {
        public long AccountLegalEntityId { get; }
        public long ULN { get; }
        public string ServiceRequestTaskId { get; }
        public string DecisionReference { get; }
        public DateTime ServiceRequestCreated { get; }

        public IEnumerable<ValidationOverrideStep> ValidationSteps { get; }

        public ValidationOverrideCommand(
            long accountLegalEntityId,
            long uln,
            string serviceRequestTaskId,
            string decisionReference,
            DateTime? serviceRequestCreated,
            IEnumerable<ValidationOverrideStep> validationSteps)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
            ServiceRequestTaskId = serviceRequestTaskId;
            DecisionReference = decisionReference;
            ServiceRequestCreated = serviceRequestCreated ?? DateTime.UtcNow;
            ValidationSteps = validationSteps;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"Validation overrides requested for ULN {ULN} in AccountLegalEntity {AccountLegalEntityId}, ServiceRequestId is {ServiceRequestTaskId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }

        public TimeSpan CommandDelay => TimeSpan.FromMinutes(2);
        public bool CancelCommand => false;
    }
}
