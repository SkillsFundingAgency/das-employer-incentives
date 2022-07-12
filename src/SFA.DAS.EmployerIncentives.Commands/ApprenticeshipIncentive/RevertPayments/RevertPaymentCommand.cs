using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments
{
    public class RevertPaymentCommand : ICommand, ILogWriter
    {
        public Guid PaymentId { get; }
        public string ServiceRequestId { get; }
        public string DecisionReferenceNumber { get; }
        public DateTime? DateServiceRequestTaskCreated { get; }

        public RevertPaymentCommand(Guid paymentId, string serviceRequestId, string decisionReferenceNumber, DateTime? dateServiceRequestTaskCreated)
        {
            PaymentId = paymentId;
            ServiceRequestId = serviceRequestId; 
            DecisionReferenceNumber = decisionReferenceNumber;
            DateServiceRequestTaskCreated = dateServiceRequestTaskCreated;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log 
        {
            get
            {
                var message = $"Revert payment requested for payment ID {PaymentId}, ServiceRequestId is {ServiceRequestId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
