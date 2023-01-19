using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.Types.PausePayments
{
    public class PausePaymentsCommand : ICommand, ILogWriter, IPeriodEndIncompatible
    {
        public long ULN { get; }
        public long AccountLegalEntityId { get; }
        public string ServiceRequestId { get; }
        public string DecisionReferenceNumber { get; }
        public DateTime? DateServiceRequestTaskCreated { get; }
        public PausePaymentsAction? Action { get; }

        public PausePaymentsCommand(long uln, long accountLegalEntityId, string serviceRequestId, string decisionReferenceNumber, DateTime? dateServiceRequestTaskCreated, PausePaymentsAction? action)
        {
            ULN = uln;
            AccountLegalEntityId = accountLegalEntityId;
            ServiceRequestId = serviceRequestId;
            DecisionReferenceNumber = decisionReferenceNumber;
            DateServiceRequestTaskCreated = dateServiceRequestTaskCreated;
            Action = action;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"Pause requested for ULN {ULN} in AccountLegalEntity {AccountLegalEntityId}, ServiceRequestId is {ServiceRequestId}";
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
