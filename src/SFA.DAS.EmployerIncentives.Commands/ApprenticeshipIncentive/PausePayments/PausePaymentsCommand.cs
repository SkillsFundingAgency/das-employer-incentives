using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments
{
    public class PausePaymentsCommand : ICommand, ILogWriter
    {
        public long ULN { get; }
        public long AccountLegalEntityId { get; }
        public string ServiceRequestId { get; }
        public string DecisionReferenceNumber { get; }
        public DateTime DateServiceRequestTaskCreated { get; }

        public PausePaymentsCommand(long uln, long accountLegalEntityId, string serviceRequestId, string decisionReferenceNumber, DateTime dateServiceRequestTaskCreated)
        {
            ULN = uln;
            AccountLegalEntityId = accountLegalEntityId;
            ServiceRequestId = serviceRequestId;
            DecisionReferenceNumber = decisionReferenceNumber;
            DateServiceRequestTaskCreated = dateServiceRequestTaskCreated;
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
    }
}
