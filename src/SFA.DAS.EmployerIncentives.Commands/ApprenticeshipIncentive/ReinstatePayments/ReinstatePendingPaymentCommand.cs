using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments
{
    public class ReinstatePendingPaymentCommand : ICommand, ILogWriter
    {
        public Guid PendingPaymentId { get; }
        public ReinstatePaymentRequest ReinstatePaymentRequest { get; }

        public ReinstatePendingPaymentCommand(Guid pendingPaymentId, ReinstatePaymentRequest reinstatePaymentRequest)
        {
            PendingPaymentId = pendingPaymentId;
            ReinstatePaymentRequest = reinstatePaymentRequest;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"Reinstate requested for pending payment ID {PendingPaymentId}, ServiceRequestId is {ReinstatePaymentRequest.TaskId} Process is {ReinstatePaymentRequest.Process}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
