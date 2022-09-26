using System;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class PendingPaymentReinstated : IDomainEvent, ILogWriter
    {
        public PendingPaymentModel Model { get; }
        public ReinstatePaymentRequest ReinstatePaymentRequest { get; }

        public PendingPaymentReinstated(PendingPaymentModel model, ReinstatePaymentRequest reinstatePaymentRequest)
        {
            Model = model;
            ReinstatePaymentRequest = reinstatePaymentRequest;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Pending Payment has been reinstated for Apprenticeship Incentive with ApprenticeshipIncentiveId {Model.ApprenticeshipIncentiveId} and " +
                    $"PendingPaymentId {Model.Id} for Service Request Id {ReinstatePaymentRequest.TaskId} Process {ReinstatePaymentRequest.Process}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
